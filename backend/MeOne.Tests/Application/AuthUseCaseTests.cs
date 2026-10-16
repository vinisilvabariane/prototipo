using MeOne.Application.Communication.Auth.Requests;
using MeOne.Application.Errors;
using MeOne.Application.UseCases.Auth;
using MeOne.Domain.Errors;
using MeOne.Domain.Rules;
using MeOne.Tests.Fakes;

namespace MeOne.Tests.Application;

public sealed class AuthUseCaseTests
{
    private static LoginUseCase Login(AccessFixture f) =>
        new(f.Users, f.Hasher, f.AccessTokens, f.RefreshTokens, f.Tokens, f.Policy);

    private static RefreshSessionUseCase Refresh(AccessFixture f) =>
        new(f.Tokens, f.Users, f.AccessTokens, f.RefreshTokens, f.AccessProfiles, f.Policy);

    private static LogoutUseCase Logout(AccessFixture f, FakeCurrentActor actor) =>
        new(f.Tokens, f.Users, f.AccessProfiles, actor);

    [Fact]
    public async Task Login_ValidCredentials_ReturnsRawTokenAndStoresOnlyHash()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345, roles: f.OperatorRole);

        var result = await Login(f).ExecuteAsync(new LoginRequest(12345, AccessFixture.Password), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal($"jwt-{user.Id}-[operador]", result.Value.Token);

        var stored = Assert.Single(f.Tokens.Stored);
        Assert.Equal(f.RefreshTokens.Hash(result.Value.RefreshToken), stored.TokenHash);
        Assert.NotEqual(stored.TokenHash, result.Value.RefreshToken);
        Assert.InRange(result.Value.RefreshExpires - DateTime.UtcNow, TimeSpan.FromHours(7.9), TimeSpan.FromHours(8));
    }

    [Fact]
    public async Task Login_WrongPassword_RejectsAndCountsAttempt()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);

        var result = await Login(f).ExecuteAsync(new LoginRequest(12345, "wrong-password"), CancellationToken.None);

        Assert.True(result.HasError<CredentialRejectedError>());
        Assert.Equal(AuthErrors.InvalidCredentials, result.Errors[0].Message);
        Assert.Equal(1, user.FailedLoginCount);
        Assert.Empty(f.Tokens.Stored);
    }

    [Fact]
    public async Task Login_TooManyFailures_LocksEvenTheRightPassword()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);

        for (var attempt = 0; attempt < LoginLockoutPolicy.MaxFailedAttempts; attempt++)
            await Login(f).ExecuteAsync(new LoginRequest(12345, "wrong-password"), CancellationToken.None);

        var result = await Login(f).ExecuteAsync(new LoginRequest(12345, AccessFixture.Password), CancellationToken.None);

        Assert.True(user.IsLockedOut(DateTime.UtcNow));
        Assert.True(result.HasError<CredentialRejectedError>());
        Assert.Equal(AuthErrors.InvalidCredentials, result.Errors[0].Message);
    }

    [Fact]
    public async Task Login_Success_ResetsFailureCount()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        await Login(f).ExecuteAsync(new LoginRequest(12345, "wrong-password"), CancellationToken.None);

        await Login(f).ExecuteAsync(new LoginRequest(12345, AccessFixture.Password), CancellationToken.None);

        Assert.Equal(0, user.FailedLoginCount);
    }

    [Fact]
    public async Task Login_UnknownRegistration_SameMessageAndStillVerifiesPassword()
    {
        var f = new AccessFixture();

        var result = await Login(f).ExecuteAsync(new LoginRequest(999, "anything"), CancellationToken.None);

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
    public async Task Login_RevokesPreviousSessions()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        var old = f.AddRefreshToken(user, "issued-old");

        await Login(f).ExecuteAsync(new LoginRequest(12345, AccessFixture.Password), CancellationToken.None);

        Assert.True(old.Revoked);
    }

    [Fact]
    public async Task Refresh_ValidToken_RotatesRevokingUsedOne()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        var used = f.AddRefreshToken(user, "issued-1");

        var result = await Refresh(f).ExecuteAsync(new RefreshTokenRequest("issued-1"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(used.Revoked);
        Assert.Contains(f.Tokens.Stored, token => token.TokenHash == f.RefreshTokens.Hash(result.Value.RefreshToken));
    }

    [Fact]
    public async Task Refresh_ReusedToken_RevokesEverySessionAndAccessTokens()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        f.AddRefreshToken(user, "issued-1");
        var first = await Refresh(f).ExecuteAsync(new RefreshTokenRequest("issued-1"), CancellationToken.None);
        var stampBefore = user.SecurityStamp;

        var reuse = await Refresh(f).ExecuteAsync(new RefreshTokenRequest("issued-1"), CancellationToken.None);

        Assert.True(reuse.IsFailed);
        Assert.All(f.Tokens.Stored, token => Assert.True(token.Revoked));
        Assert.NotEqual(stampBefore, user.SecurityStamp);
        Assert.Contains(user.Id, f.AccessProfiles.Invalidated);
        Assert.True((await Refresh(f).ExecuteAsync(new RefreshTokenRequest(first.Value.RefreshToken), CancellationToken.None)).IsFailed);
    }

    [Fact]
    public async Task Refresh_ExpiredToken_FailsWith400()
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
    public async Task Refresh_DeactivatedUser_Fails()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        f.AddRefreshToken(user, "issued-1");
        user.Deactivate(modifiedBy: 1);

        var result = await Refresh(f).ExecuteAsync(new RefreshTokenRequest("issued-1"), CancellationToken.None);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task Logout_OwnUser_RevokesSessionsAndRotatesStamp()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        var token = f.AddRefreshToken(user, "issued-1");
        var stampBefore = user.SecurityStamp;

        var result = await Logout(f, FakeCurrentActor.For(user)).ExecuteAsync(new LogoutRequest(user.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(token.Revoked);
        Assert.NotEqual(stampBefore, user.SecurityStamp);
        Assert.Contains(user.Id, f.AccessProfiles.Invalidated);
    }

    [Fact]
    public async Task Logout_OtherUserWithoutPermission_Forbidden()
    {
        var f = new AccessFixture();
        var target = f.AddUser(12345);
        var actor = f.AddUser(54321, "other@grupomultilaser.com.br", roles: f.OperatorRole);
        var token = f.AddRefreshToken(target, "issued-1");

        var result = await Logout(f, FakeCurrentActor.For(actor)).ExecuteAsync(new LogoutRequest(target.Id), CancellationToken.None);

        Assert.True(result.HasError<ForbiddenError>());
        Assert.False(token.Revoked);
    }

    [Fact]
    public async Task Logout_AccessManagerCannotLogOutAdministrator()
    {
        var f = new AccessFixture();
        var admin = f.AddAdministrator();
        var manager = f.AddUser(54321, "manager@grupomultilaser.com.br", roles: f.AccessManagerRole);

        var result = await Logout(f, FakeCurrentActor.For(manager)).ExecuteAsync(new LogoutRequest(admin.Id), CancellationToken.None);

        Assert.True(result.HasError<ForbiddenError>());
    }

    [Fact]
    public async Task Logout_AdministratorLogsOutOtherUser()
    {
        var f = new AccessFixture();
        var target = f.AddUser(12345);
        var token = f.AddRefreshToken(target, "issued-1");

        var result = await Logout(f, FakeCurrentActor.Administrator()).ExecuteAsync(new LogoutRequest(target.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(token.Revoked);
    }

    [Fact]
    public async Task ValidateCredentials_OwnPassword_PassesWithoutOpeningSession()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        var useCase = new ValidateCredentialsUseCase(f.Users, f.Hasher, FakeCurrentActor.For(user));

        var ok = await useCase.ExecuteAsync(new ValidateCredentialsRequest(12345, AccessFixture.Password), CancellationToken.None);
        var wrong = await useCase.ExecuteAsync(new ValidateCredentialsRequest(12345, "x"), CancellationToken.None);

        Assert.True(ok.IsSuccess);
        Assert.True(wrong.HasError<CredentialRejectedError>());
        Assert.Empty(f.Tokens.Stored);
    }

    [Fact]
    public async Task ValidateCredentials_OtherUsersPassword_Forbidden()
    {
        var f = new AccessFixture();
        var actor = f.AddUser(12345);
        f.AddUser(54321, "other@grupomultilaser.com.br");
        var useCase = new ValidateCredentialsUseCase(f.Users, f.Hasher, FakeCurrentActor.For(actor));

        var result = await useCase.ExecuteAsync(new ValidateCredentialsRequest(54321, AccessFixture.Password), CancellationToken.None);

        Assert.True(result.HasError<ForbiddenError>());
        Assert.Equal(0, f.Hasher.VerifyCalls);
    }

    [Fact]
    public async Task GetMyAccess_ReturnsAssignmentsAndScopedGrants()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        user.GrantRole(f.SupervisorRole, f.AreaA);

        var result = await new GetMyAccessUseCase(f.Users, FakeCurrentActor.For(user)).ExecuteAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        var assignment = Assert.Single(result.Value.Roles);
        Assert.Equal("supervisor-producao", assignment.Name);
        Assert.Equal(f.AreaA.Id, assignment.ScopeId);
        Assert.Contains(result.Value.Permissions, grant => grant.Permission == Permissions.ProductionSupervise && grant.ScopeId == f.AreaA.Id);
        Assert.False(result.Value.IsAdministrator);
    }
}
