using IrisWeb.API.Auth;
using IrisWeb.API.Errors;
using IrisWeb.Application.Communication.Collaborators.Responses;
using IrisWeb.Application.Interfaces.Collaborators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace IrisWeb.API.Controllers;

/// <summary>Consulta a base de colaboradores do Sync; anônima porque o autocadastro usa para preencher o nome.</summary>
[ApiController]
[Route("api/v1/colaboradores")]
public class CollaboratorsController(IGetCollaboratorUseCase getCollaborator) : ControllerBase
{
    [HttpGet("get-by-registration/{registration}")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthPolicies.AnonymousRateLimit)]
    [ProducesResponseType(typeof(CollaboratorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<CollaboratorResponse>> GetByRegistration(
        uint registration,
        CancellationToken cancellationToken)
    {
        var result = await getCollaborator.ExecuteAsync(registration, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Falha ao consultar colaborador", result);
    }
}
