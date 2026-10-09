using FluentResults;
using IrisWeb.API.Errors;
using IrisWeb.Application.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IrisWeb.Tests.API;

public sealed class ApiResultsTests
{
    [Fact]
    public void Failure_DependenciaExternaForaDoAr_Responde503()
    {
        var result = Result.Fail(new UpstreamUnavailableError("A pasta de artefatos nao respondeu."));

        var action = ApiResults.Failure("Storage indisponivel", result);

        var objectResult = Assert.IsType<ObjectResult>(action);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, objectResult.StatusCode);

        var body = Assert.IsType<ErrorResponse>(objectResult.Value);
        Assert.Equal("Storage indisponivel", body.Error);
        Assert.Equal("A pasta de artefatos nao respondeu.", body.Details);
    }

    [Fact]
    public void Failure_RecusaDeNegocio_Responde400()
    {
        var result = Result.Fail("recusa");

        var action = ApiResults.Failure("Falha de negocio", result);

        var badRequest = Assert.IsType<BadRequestObjectResult>(action);

        var body = Assert.IsType<ErrorResponse>(badRequest.Value);
        Assert.Equal("Falha de negocio", body.Error);
        Assert.Equal("recusa", body.Details);
    }

    [Fact]
    public void Failure_NaoEncontrado_Responde404()
    {
        var action = ApiResults.Failure("Falha", Result.Fail(new NotFoundError("Usuário não encontrado.")));

        var notFound = Assert.IsType<NotFoundObjectResult>(action);
        Assert.Equal("Usuário não encontrado.", Assert.IsType<ErrorResponse>(notFound.Value).Details);
    }

    [Fact]
    public void Failure_Proibido_Responde403()
    {
        var action = ApiResults.Failure("Falha", Result.Fail(new ForbiddenError("so master")));

        var objectResult = Assert.IsType<ObjectResult>(action);
        Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
    }
}
