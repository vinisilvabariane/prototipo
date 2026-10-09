using FluentResults;
using IrisWeb.Application.Errors;
using Microsoft.AspNetCore.Mvc;

namespace IrisWeb.API.Errors;

public static class ApiResults
{
    public static ActionResult Failure(string title, ResultBase result)
    {
        var body = ErrorResponse.From(title, result.Errors.Select(error => error.Message));

        if (result.HasError<CredentialRejectedError>())
            return new ObjectResult(body) { StatusCode = StatusCodes.Status401Unauthorized };

        if (result.HasError<ForbiddenError>())
            return new ObjectResult(body) { StatusCode = StatusCodes.Status403Forbidden };

        if (result.HasError<NotFoundError>())
            return new NotFoundObjectResult(body);

        if (result.HasError<UpstreamGatewayError>())
            return new ObjectResult(body) { StatusCode = StatusCodes.Status502BadGateway };

        return result.HasError<UpstreamUnavailableError>()
            ? new ObjectResult(body) { StatusCode = StatusCodes.Status503ServiceUnavailable }
            : new BadRequestObjectResult(body);
    }
}
