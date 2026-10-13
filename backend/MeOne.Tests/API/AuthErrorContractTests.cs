using System.Text;
using System.Text.Json;
using MeOne.API.Controllers;
using MeOne.Application.Communication.Auth.Requests;
using MeOne.Application.UseCases.Auth;
using MeOne.Domain.Errors;
using MeOne.Tests.Fakes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace MeOne.Tests.API;

public sealed class AuthErrorContractTests
{
    private sealed record Response(int Status, string? ContentType, string Body)
    {
        public JsonElement Json => JsonSerializer.Deserialize<JsonElement>(Body);

        public string Error => Json.GetProperty("error").GetString()!;

        public string Details => Json.GetProperty("details").GetString()!;
    }

    private static AuthController Controller(AccessFixture f, FakeCurrentActor actor) => new(
        new LoginUseCase(f.Users, f.Hasher, f.AccessTokens, f.RefreshTokens, f.Tokens, f.Policy),
        new RefreshSessionUseCase(f.Tokens, f.Users, f.AccessTokens, f.RefreshTokens, f.Policy),
        new LogoutUseCase(f.Tokens, actor),
        new ValidateCredentialsUseCase(f.Users, f.Hasher));

    private static async Task<Response> RenderAsync(IActionResult actionResult)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMvcCore();

        using var provider = services.BuildServiceProvider();

        var context = new DefaultHttpContext { RequestServices = provider };
        context.Request.Path = "/api/v1/auth";

        using var body = new MemoryStream();
        context.Response.Body = body;

        await actionResult.ExecuteResultAsync(new ActionContext(context, new RouteData(), new ActionDescriptor()));

        return new Response(
            context.Response.StatusCode,
            context.Response.ContentType,
            Encoding.UTF8.GetString(body.ToArray()));
    }

    [Fact]
    public async Task RejectedCredential_Returns401WithErrorEnvelope()
    {
        var f = new AccessFixture();
        f.AddUser(12345);

        var actionResult = await Controller(f, FakeCurrentActor.Anonymous())
            .Login(new LoginRequest(12345, "wrong-password"), CancellationToken.None);

        var response = await RenderAsync(actionResult.Result!);

        Assert.Equal(StatusCodes.Status401Unauthorized, response.Status);
        Assert.Contains("application/json", response.ContentType);
        Assert.False(string.IsNullOrWhiteSpace(response.Error));
        Assert.Equal(AuthErrors.InvalidCredentials, response.Details);
    }

    [Fact]
    public async Task UnknownRegistration_RespondsLikeWrongPassword()
    {
        var f = new AccessFixture();

        var actionResult = await Controller(f, FakeCurrentActor.Anonymous())
            .Login(new LoginRequest(999, "anything"), CancellationToken.None);

        var response = await RenderAsync(actionResult.Result!);

        Assert.Equal(StatusCodes.Status401Unauthorized, response.Status);
        Assert.Equal(AuthErrors.InvalidCredentials, response.Details);
    }

    [Fact]
    public async Task InvalidRefresh_Returns400SoInterceptorDoesNotLoop()
    {
        var f = new AccessFixture();

        var actionResult = await Controller(f, FakeCurrentActor.Anonymous())
            .Refresh(new RefreshTokenRequest("does-not-exist"), CancellationToken.None);

        var response = await RenderAsync(actionResult.Result!);

        Assert.Equal(StatusCodes.Status400BadRequest, response.Status);
        Assert.Equal(AuthErrors.InvalidRefreshToken, response.Details);
    }

    [Fact]
    public async Task LogoutOtherUser_Returns403WithEnvelope()
    {
        var f = new AccessFixture();
        var target = f.AddUser(12345);

        var actionResult = await Controller(f, FakeCurrentActor.Common(userId: 999))
            .Logout(new LogoutRequest(target.Id), CancellationToken.None);

        var response = await RenderAsync(actionResult);

        Assert.Equal(StatusCodes.Status403Forbidden, response.Status);
        Assert.Equal(AuthErrors.LogoutOtherUserForbidden, response.Details);
    }
}
