using MeOne.Application.Communication.Auth.Requests;
using MeOne.Application.Errors;
using MeOne.Application.UseCases.Auth;
using MeOne.Domain.Errors;
using MeOne.Tests.Fakes;

namespace MeOne.Tests.Application;

public sealed class AuthUseCaseTests
{
    private static LoginUseCase Login(AccessFixture f) =>
        new(f.Users, f.Hasher, f.AccessTokens, f.RefreshTokens, f.Tokens, f.Policy);

    private static RefreshSessionUseCase Refresh(AccessFixture f) =>
        new(f.Tokens, f.Users, f.AccessTokens, f.RefreshTokens, f.Policy);

    [Fact]
    public async Task Login_ValidCredentials_ReturnsRawTokenAndStoresOnlyHash()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345, roles: f.MasterRole);

        var result = await Login(f).ExecuteAsync(new LoginRequest(12345, AccessFixture.Password), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal($"jwt-{user.Id}-[master]", result.Value.Token);

        var stored = Assert.Single(f.Tokens.Stored);
        Assert.Equal(f.RefreshTokens.Hash(result.Value.RefreshToken), stored.TokenHash);
        Assert.NotEqual(stored.TokenHash, result.Value.RefreshToken);
        Assert.Equal(user.Id, stored.UserId);
        Assert.Equal(stored.ExpiresAt, result.Value.RefreshExpires);
        Assert.InRange(result.Value.RefreshExpires - DateTime.UtcNow, TimeSpan.FromHours(7.9), TimeSpan.FromHours(8));
    }

    [Fact]
    public async Task Login_WrongPassword_RejectsCredential()
    {
        var f = new AccessFixture();
        f.AddUser(12345);

        var result = await Login(f).ExecuteAsync(new LoginRequest(12345, "wrong-password"), CancellationToken.None);

        Assert.True(result.HasError<CredentialRejectedError>());
        Assert.Equal(AuthErrors.InvalidCredentials, result.Errors[0].Message);
        Assert.Empty(f.Tokens.Stored);
    }

    [Fact]
    public async Task Login_UnknownRegistration_RejectsWithSameMessageAndStillVerifiesPassword()
    {
        var f = new AccessFixture();

        var result = await Login(f).ExecuteAsync(new LoginRequest(999, "anything"), CancellationToken.None);

        Assert.True(result.HasError<CredentialRejectedError>());
        Assert.Equal(AuthErrors.InvalidCredentials, result.Errors[0].Message);
        Assert.Equal(1, f.Hasher.VerifyCalls);
    }

    [Fact]
    public async Task Login_DeactivatedUser_CannotSignIn()
    {
        var f = new AccessFixture();
        f.AddUser(12345).Deactivate(modifiedBy: 1);

        var result = await Login(f).ExecuteAsync(new LoginRequest(12345, AccessFixture.Password), CancellationToken.None);

        Assert.True(result.HasError<CredentialRejectedError>());
    }

    [Fact]
    public async Task Login_RevokesPreviousUserSessions()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        var oldToken = f.AddRefreshToken(user, "old");

        var result = await Login(f).ExecuteAsync(new LoginRequest(12345, AccessFixture.Password), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(oldToken.Revoked);
    }

    [Fact]
    public async Task Refresh_ValidToken_RotatesRevokingUsedOne()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        var usedToken = f.AddRefreshToken(user, "issued-1");

        var result = await Refresh(f).ExecuteAsync(new RefreshTokenRequest("issued-1"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(usedToken.Revoked);
        Assert.NotEqual("issued-1", result.Value.RefreshToken);
        Assert.Contains(f.Tokens.Stored, token => token.TokenHash == f.RefreshTokens.Hash(result.Value.RefreshToken));
    }

    [Fact]
    public async Task Refresh_SameTokenTwice_SecondFails()
    {
        var f = new AccessFixture();
        f.AddRefreshToken(f.AddUser(12345), "issued-1");

        await Refresh(f).ExecuteAsync(new RefreshTokenRequest("issued-1"), CancellationToken.None);
        var second = await Refresh(f).ExecuteAsync(new RefreshTokenRequest("issued-1"), CancellationToken.None);

        Assert.True(second.IsFailed);
        Assert.Equal(AuthErrors.InvalidRefreshToken, second.Errors[0].Message);
    }

    [Fact]
    public async Task Refresh_ExpiredToken_Fails()
    {
        var f = new AccessFixture();
        f.AddRefreshToken(f.AddUser(12345), "issued-1", DateTime.UtcNow.AddMinutes(-1));

        var result = await Refresh(f).ExecuteAsync(new RefreshTokenRequest("issued-1"), CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.False(result.HasError<CredentialRejectedError>());
    }

    [Fact]
    public async Task Refresh_HashSentInsteadOfToken_NotFound()
    {
        var f = new AccessFixture();
        f.AddRefreshToken(f.AddUser(12345), "issued-1");

        var result = await Refresh(f).ExecuteAsync(
            new RefreshTokenRequest(f.RefreshTokens.Hash("issued-1")),
            CancellationToken.None);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task Refresh_DeactivatedUser_FailsAndRevokesToken()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        var token = f.AddRefreshToken(user, "issued-1");
        user.Deactivate(modifiedBy: 1);

        var result = await Refresh(f).ExecuteAsync(new RefreshTokenRequest("issued-1"), CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(token.Revoked);
    }

    [Fact]
    public async Task Logout_OwnUser_RevokesSessions()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        var token = f.AddRefreshToken(user, "issued-1");

        var result = await new LogoutUseCase(f.Tokens, FakeCurrentActor.Common(user.Id))
            .ExecuteAsync(new LogoutRequest(user.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(token.Revoked);
    }

    [Fact]
    public async Task Logout_OtherUserWithoutMaster_Forbidden()
    {
        var f = new AccessFixture();
        var target = f.AddUser(12345);
        var token = f.AddRefreshToken(target, "issued-1");

        var result = await new LogoutUseCase(f.Tokens, FakeCurrentActor.Common(userId: 999))
            .ExecuteAsync(new LogoutRequest(target.Id), CancellationToken.None);

        Assert.True(result.HasError<ForbiddenError>());
        Assert.False(token.Revoked);
    }

    [Fact]
    public async Task Logout_MasterRevokesOtherUserSession()
    {
        var f = new AccessFixture();
        var target = f.AddUser(12345);
        var token = f.AddRefreshToken(target, "issued-1");

        var result = await new LogoutUseCase(f.Tokens, FakeCurrentActor.Master(userId: 999))
            .ExecuteAsync(new LogoutRequest(target.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(token.Revoked);
    }

    [Fact]
    public async Task Logout_WithoutActiveSession_StillSucceeds()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);

        var result = await new LogoutUseCase(f.Tokens, FakeCurrentActor.Common(user.Id))
            .ExecuteAsync(new LogoutRequest(user.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ValidateCredentials_ChecksWithoutOpeningSession()
    {
        var f = new AccessFixture();
        f.AddUser(12345);
        var useCase = new ValidateCredentialsUseCase(f.Users, f.Hasher);

        var ok = await useCase.ExecuteAsync(new ValidateCredentialsRequest(12345, AccessFixture.Password), CancellationToken.None);
        var wrong = await useCase.ExecuteAsync(new ValidateCredentialsRequest(12345, "x"), CancellationToken.None);

        Assert.True(ok.IsSuccess);
        Assert.True(wrong.HasError<CredentialRejectedError>());
        Assert.Empty(f.Tokens.Stored);
    }
}
