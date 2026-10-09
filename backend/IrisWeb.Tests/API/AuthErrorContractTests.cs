using System.Text;
using System.Text.Json;
using IrisWeb.API.Controllers;
using IrisWeb.Application.Communication.Auth.Requests;
using IrisWeb.Application.UseCases.Auth;
using IrisWeb.Domain.Errors;
using IrisWeb.Tests.Fakes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace IrisWeb.Tests.API;

public sealed class AuthErrorContractTests
{
    private sealed record Resposta(int Status, string? ContentType, string Corpo)
    {
        public JsonElement Json => JsonSerializer.Deserialize<JsonElement>(Corpo);

        public string Error => Json.GetProperty("error").GetString()!;

        public string Details => Json.GetProperty("details").GetString()!;
    }

    private static AuthController Controller(AccessFixture f, FakeCurrentActor actor) => new(
        new LoginUseCase(f.Users, f.Hasher, f.AccessTokens, f.RefreshTokens, f.Tokens, f.Policy),
        new RefreshSessionUseCase(f.Tokens, f.Users, f.AccessTokens, f.RefreshTokens, f.Policy),
        new LogoutUseCase(f.Tokens, actor),
        new ValidateCredentialsUseCase(f.Users, f.Hasher));

    private static async Task<Resposta> ExecutarAsync(IActionResult acao)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMvcCore();

        using var provider = services.BuildServiceProvider();

        var context = new DefaultHttpContext { RequestServices = provider };
        context.Request.Path = "/api/v1/auth";

        using var corpo = new MemoryStream();
        context.Response.Body = corpo;

        await acao.ExecuteResultAsync(new ActionContext(context, new RouteData(), new ActionDescriptor()));

        return new Resposta(
            context.Response.StatusCode,
            context.Response.ContentType,
            Encoding.UTF8.GetString(corpo.ToArray()));
    }

    [Fact]
    public async Task CredencialRecusada_Responde401ComEnvelopeDeErro()
    {
        var f = new AccessFixture();
        f.AddUser(12345);

        var acao = await Controller(f, FakeCurrentActor.Anonymous())
            .Login(new LoginRequest(12345, "errada"), CancellationToken.None);

        var resposta = await ExecutarAsync(acao.Result!);

        Assert.Equal(StatusCodes.Status401Unauthorized, resposta.Status);
        Assert.Contains("application/json", resposta.ContentType);
        Assert.False(string.IsNullOrWhiteSpace(resposta.Error));
        Assert.Equal(AuthErrors.InvalidCredentials, resposta.Details);
    }

    [Fact]
    public async Task MatriculaInexistente_RespondeIgualASenhaErrada()
    {
        var f = new AccessFixture();

        var acao = await Controller(f, FakeCurrentActor.Anonymous())
            .Login(new LoginRequest(999, "qualquer"), CancellationToken.None);

        var resposta = await ExecutarAsync(acao.Result!);

        Assert.Equal(StatusCodes.Status401Unauthorized, resposta.Status);
        Assert.Equal(AuthErrors.InvalidCredentials, resposta.Details);
    }

    [Fact]
    public async Task RefreshInvalido_Responde400ParaOInterceptorNaoEntrarEmLaco()
    {
        var f = new AccessFixture();

        var acao = await Controller(f, FakeCurrentActor.Anonymous())
            .Refresh(new RefreshTokenRequest("nao-existe"), CancellationToken.None);

        var resposta = await ExecutarAsync(acao.Result!);

        Assert.Equal(StatusCodes.Status400BadRequest, resposta.Status);
        Assert.Equal(AuthErrors.InvalidRefreshToken, resposta.Details);
    }

    [Fact]
    public async Task LogoutDeOutroUsuario_Responde403ComEnvelope()
    {
        var f = new AccessFixture();
        var alvo = f.AddUser(12345);

        var acao = await Controller(f, FakeCurrentActor.Common(userId: 999))
            .Logout(new LogoutRequest(alvo.Id), CancellationToken.None);

        var resposta = await ExecutarAsync(acao);

        Assert.Equal(StatusCodes.Status403Forbidden, resposta.Status);
        Assert.Equal(AuthErrors.LogoutOtherUserForbidden, resposta.Details);
    }
}
