using FluentResults;
using MeOne.API.Errors;
using MeOne.Application.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MeOne.Tests.API;

public sealed class ApiResultsTests
{
    [Fact]
    public void Failure_ExternalDependencyDown_Returns503()
    {
        var result = Result.Fail(new UpstreamUnavailableError("The email server did not respond."));

        var action = ApiResults.Failure("Dependency unavailable", result);

        var objectResult = Assert.IsType<ObjectResult>(action);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, objectResult.StatusCode);

        var body = Assert.IsType<ErrorResponse>(objectResult.Value);
        Assert.Equal("Dependency unavailable", body.Error);
        Assert.Equal("The email server did not respond.", body.Details);
    }

    [Fact]
    public void Failure_BusinessRejection_Returns400()
    {
        var result = Result.Fail("rejection");

        var action = ApiResults.Failure("Business failure", result);

        var badRequest = Assert.IsType<BadRequestObjectResult>(action);

        var body = Assert.IsType<ErrorResponse>(badRequest.Value);
        Assert.Equal("Business failure", body.Error);
        Assert.Equal("rejection", body.Details);
    }

    [Fact]
    public void Failure_NotFound_Returns404()
    {
        var action = ApiResults.Failure("Failure", Result.Fail(new NotFoundError("Usuário não encontrado.")));

        var notFound = Assert.IsType<NotFoundObjectResult>(action);
        Assert.Equal("Usuário não encontrado.", Assert.IsType<ErrorResponse>(notFound.Value).Details);
    }

    [Fact]
    public void Failure_Forbidden_Returns403()
    {
        var action = ApiResults.Failure("Failure", Result.Fail(new ForbiddenError("master only")));

        var objectResult = Assert.IsType<ObjectResult>(action);
        Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
    }
}
