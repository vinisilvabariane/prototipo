using IrisWeb.Application.Communication.Auth.Requests;
using IrisWeb.Application.Errors;
using IrisWeb.Application.UseCases.Auth;
using IrisWeb.Domain.Errors;
using IrisWeb.Tests.Fakes;

namespace IrisWeb.Tests.Application;

public sealed class AuthUseCaseTests
{
    private static LoginUseCase Login(AccessFixture f) =>
        new(f.Users, f.Hasher, f.AccessTokens, f.RefreshTokens, f.Tokens, f.Policy);

    private static RefreshSessionUseCase Refresh(AccessFixture f) =>
        new(f.Tokens, f.Users, f.AccessTokens, f.RefreshTokens, f.Policy);

    [Fact]
    public async Task Login_CredenciaisCertas_DevolveTokenCruEGuardaSoOHash()
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
    public async Task Login_SenhaErrada_RecusaCredencial()
    {
        var f = new AccessFixture();
        f.AddUser(12345);

        var result = await Login(f).ExecuteAsync(new LoginRequest(12345, "errada"), CancellationToken.None);

        Assert.True(result.HasError<CredentialRejectedError>());
        Assert.Equal(AuthErrors.InvalidCredentials, result.Errors[0].Message);
        Assert.Empty(f.Tokens.Stored);
    }

    [Fact]
    public async Task Login_MatriculaInexistente_RecusaComAMesmaMensagemEAindaVerificaSenha()
    {
        var f = new AccessFixture();

        var result = await Login(f).ExecuteAsync(new LoginRequest(999, "qualquer"), CancellationToken.None);

        Assert.True(result.HasError<CredentialRejectedError>());
        Assert.Equal(AuthErrors.InvalidCredentials, result.Errors[0].Message);
        Assert.Equal(1, f.Hasher.VerifyCalls);
    }

    [Fact]
    public async Task Login_UsuarioDesativado_NaoEntra()
    {
        var f = new AccessFixture();
        f.AddUser(12345).Deactivate(modifiedBy: 1);

        var result = await Login(f).ExecuteAsync(new LoginRequest(12345, AccessFixture.Password), CancellationToken.None);

        Assert.True(result.HasError<CredentialRejectedError>());
    }

    [Fact]
    public async Task Login_DerrubaAsSessoesAnterioresDoUsuario()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        var antigo = f.AddRefreshToken(user, "antigo");

        var result = await Login(f).ExecuteAsync(new LoginRequest(12345, AccessFixture.Password), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(antigo.Revoked);
    }

    [Fact]
    public async Task Refresh_TokenValido_RotacionaRevogandoOUsado()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        var usado = f.AddRefreshToken(user, "cru-1");

        var result = await Refresh(f).ExecuteAsync(new RefreshTokenRequest("cru-1"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(usado.Revoked);
        Assert.NotEqual("cru-1", result.Value.RefreshToken);
        Assert.Contains(f.Tokens.Stored, token => token.TokenHash == f.RefreshTokens.Hash(result.Value.RefreshToken));
    }

    [Fact]
    public async Task Refresh_MesmoTokenDuasVezes_SegundaFalha()
    {
        var f = new AccessFixture();
        f.AddRefreshToken(f.AddUser(12345), "cru-1");

        await Refresh(f).ExecuteAsync(new RefreshTokenRequest("cru-1"), CancellationToken.None);
        var segunda = await Refresh(f).ExecuteAsync(new RefreshTokenRequest("cru-1"), CancellationToken.None);

        Assert.True(segunda.IsFailed);
        Assert.Equal(AuthErrors.InvalidRefreshToken, segunda.Errors[0].Message);
    }

    [Fact]
    public async Task Refresh_TokenVencido_Falha()
    {
        var f = new AccessFixture();
        f.AddRefreshToken(f.AddUser(12345), "cru-1", DateTime.UtcNow.AddMinutes(-1));

        var result = await Refresh(f).ExecuteAsync(new RefreshTokenRequest("cru-1"), CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.False(result.HasError<CredentialRejectedError>());
    }

    [Fact]
    public async Task Refresh_HashEnviadoNoLugarDoToken_NaoEncontra()
    {
        // O Access Control devolvia o hash ao cliente; o hash não pode servir de token.
        var f = new AccessFixture();
        f.AddRefreshToken(f.AddUser(12345), "cru-1");

        var result = await Refresh(f).ExecuteAsync(
            new RefreshTokenRequest(f.RefreshTokens.Hash("cru-1")),
            CancellationToken.None);

        Assert.True(result.IsFailed);
    }

    [Fact]
    public async Task Refresh_UsuarioDesativado_FalhaERevogaOToken()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        var token = f.AddRefreshToken(user, "cru-1");
        user.Deactivate(modifiedBy: 1);

        var result = await Refresh(f).ExecuteAsync(new RefreshTokenRequest("cru-1"), CancellationToken.None);

        Assert.True(result.IsFailed);
        Assert.True(token.Revoked);
    }

    [Fact]
    public async Task Logout_ProprioUsuario_RevogaAsSessoes()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        var token = f.AddRefreshToken(user, "cru-1");

        var result = await new LogoutUseCase(f.Tokens, FakeCurrentActor.Common(user.Id))
            .ExecuteAsync(new LogoutRequest(user.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(token.Revoked);
    }

    [Fact]
    public async Task Logout_DeOutroUsuarioSemSerMaster_Proibido()
    {
        var f = new AccessFixture();
        var alvo = f.AddUser(12345);
        var token = f.AddRefreshToken(alvo, "cru-1");

        var result = await new LogoutUseCase(f.Tokens, FakeCurrentActor.Common(userId: 999))
            .ExecuteAsync(new LogoutRequest(alvo.Id), CancellationToken.None);

        Assert.True(result.HasError<ForbiddenError>());
        Assert.False(token.Revoked);
    }

    [Fact]
    public async Task Logout_MasterDerrubaSessaoDeOutro()
    {
        var f = new AccessFixture();
        var alvo = f.AddUser(12345);
        var token = f.AddRefreshToken(alvo, "cru-1");

        var result = await new LogoutUseCase(f.Tokens, FakeCurrentActor.Master(userId: 999))
            .ExecuteAsync(new LogoutRequest(alvo.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(token.Revoked);
    }

    [Fact]
    public async Task Logout_SemSessaoAtiva_AindaESucesso()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);

        var result = await new LogoutUseCase(f.Tokens, FakeCurrentActor.Common(user.Id))
            .ExecuteAsync(new LogoutRequest(user.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ValidateCredentials_ConfereSemAbrirSessao()
    {
        var f = new AccessFixture();
        f.AddUser(12345);
        var useCase = new ValidateCredentialsUseCase(f.Users, f.Hasher);

        var ok = await useCase.ExecuteAsync(new ValidateCredentialsRequest(12345, AccessFixture.Password), CancellationToken.None);
        var errada = await useCase.ExecuteAsync(new ValidateCredentialsRequest(12345, "x"), CancellationToken.None);

        Assert.True(ok.IsSuccess);
        Assert.True(errada.HasError<CredentialRejectedError>());
        Assert.Empty(f.Tokens.Stored);
    }
}
