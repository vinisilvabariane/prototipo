using FluentValidation;
using MeOne.API.Errors;
using MeOne.API.Validation;
using MeOne.Application.Communication.Auth.Requests;
using MeOne.Application.Validators.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace MeOne.Tests.API;

public sealed class FluentValidationFilterTests
{
    private static async Task<(ActionExecutingContext Context, bool ActionRan)> RunAsync(params object?[] arguments)
    {
        var services = new ServiceCollection()
            .AddValidatorsFromAssemblyContaining<LoginRequestValidator>()
            .BuildServiceProvider();

        var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
        var actionArguments = arguments
            .Select((value, index) => (Key: $"arg{index}", Value: value))
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        var context = new ActionExecutingContext(actionContext, [], actionArguments, controller: new object());

        var actionRan = false;
        await new FluentValidationFilter(services).OnActionExecutionAsync(context, () =>
        {
            actionRan = true;
            return Task.FromResult(new ActionExecutedContext(actionContext, [], controller: new object()));
        });

        return (context, actionRan);
    }

    [Fact]
    public async Task InvalidRequest_Returns400WithEnvelopeAndSkipsAction()
    {
        var (context, actionRan) = await RunAsync(new LoginRequest(0, ""));

        Assert.False(actionRan);
        var badRequest = Assert.IsType<BadRequestObjectResult>(context.Result);
        var body = Assert.IsType<ErrorResponse>(badRequest.Value);
        Assert.Equal(FluentValidationFilter.Title, body.Error);
        Assert.Contains("Informe a senha.", body.Details);
        Assert.Contains("A matrícula deve estar entre 1 e 999999.", body.Details);
    }

    [Fact]
    public async Task ValidRequest_ContinuesToAction()
    {
        var (context, actionRan) = await RunAsync(new LoginRequest(12345, "password"));

        Assert.True(actionRan);
        Assert.Null(context.Result);
    }

    [Fact]
    public async Task ArgumentWithoutValidatorOrNull_IsIgnored()
    {
        var (context, actionRan) = await RunAsync(42UL, null, "text");

        Assert.True(actionRan);
        Assert.Null(context.Result);
    }

    [Fact]
    public async Task MultipleArguments_CollectsAllErrors()
    {
        var (context, _) = await RunAsync(new LoginRequest(0, "password"), new RefreshTokenRequest(""));

        var body = Assert.IsType<ErrorResponse>(Assert.IsType<BadRequestObjectResult>(context.Result).Value);
        Assert.Contains("A matrícula deve estar entre 1 e 999999.", body.Details);
        Assert.Contains("Informe o refresh token.", body.Details);
    }
}
