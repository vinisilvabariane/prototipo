using MeOne.API.Auth;
using MeOne.API.Errors;
using MeOne.Application.Communication.Roles.Responses;
using MeOne.Application.Interfaces.Roles;
using MeOne.Domain.Rules;
using Microsoft.AspNetCore.Mvc;

namespace MeOne.API.Controllers;

[ApiController]
[Route("api/v1/permissions")]
public class PermissionsController(IListPermissionsUseCase listPermissions) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.RolesRead)]
    [ProducesResponseType(typeof(IReadOnlyList<PermissionResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PermissionResponse>>> List(CancellationToken cancellationToken)
    {
        var result = await listPermissions.ExecuteAsync(cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Erro ao buscar permissões", result);
    }
}
