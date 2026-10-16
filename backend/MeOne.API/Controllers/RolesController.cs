using MeOne.API.Auth;
using MeOne.API.Errors;
using MeOne.Application.Communication.Common.Responses;
using MeOne.Application.Communication.Roles.Requests;
using MeOne.Application.Communication.Roles.Responses;
using MeOne.Application.Interfaces.Roles;
using MeOne.Domain.Rules;
using Microsoft.AspNetCore.Mvc;

namespace MeOne.API.Controllers;

[ApiController]
[Route("api/v1/roles")]
public class RolesController(
    IListRolesUseCase listRoles,
    IListAllRolesUseCase listAllRoles,
    IGetRoleUseCase getRole,
    ICreateRoleUseCase createRole,
    IUpdateRoleUseCase updateRole,
    IDeleteRoleUseCase deleteRole,
    ISetRolePermissionsUseCase setRolePermissions,
    IAssignRoleUseCase assignRole,
    IRevokeRoleUseCase revokeRole) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.RolesRead)]
    [ProducesResponseType(typeof(PagedResponse<RoleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<RoleResponse>>> List(
        [FromQuery] RolesQuery query,
        CancellationToken cancellationToken)
    {
        var result = await listRoles.ExecuteAsync(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Erro ao buscar papéis", result);
    }

    [HttpGet("all")]
    [RequirePermission(Permissions.RolesRead)]
    [ProducesResponseType(typeof(IReadOnlyList<RoleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RoleResponse>>> All(CancellationToken cancellationToken)
    {
        var result = await listAllRoles.ExecuteAsync(cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Erro ao buscar papéis", result);
    }

    [HttpGet("{id:long}")]
    [RequirePermission(Permissions.RolesRead)]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoleResponse>> GetById(ulong id, CancellationToken cancellationToken)
    {
        var result = await getRole.ExecuteAsync(id, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Falha ao buscar papel", result);
    }

    [HttpPost]
    [RequirePermission(Permissions.RolesManage)]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<RoleResponse>> Create(CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await createRole.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : ApiResults.Failure("Falha ao criar papel", result);
    }

    [HttpPut("{id:long}")]
    [RequirePermission(Permissions.RolesManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(ulong id, UpdateRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await updateRole.ExecuteAsync(id, request, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao atualizar papel", result);
    }

    [HttpPut("{id:long}/permissions")]
    [RequirePermission(Permissions.RolesManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetPermissions(
        ulong id,
        SetRolePermissionsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await setRolePermissions.ExecuteAsync(id, request, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao definir permissões do papel", result);
    }

    [HttpDelete("{id:long}")]
    [RequirePermission(Permissions.RolesManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(ulong id, CancellationToken cancellationToken)
    {
        var result = await deleteRole.ExecuteAsync(id, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao excluir papel", result);
    }

    [HttpPost("set-role")]
    [RequirePermission(Permissions.RolesAssign)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Assign(AssignRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await assignRole.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao atribuir papel", result);
    }

    [HttpPost("revoke-role")]
    [RequirePermission(Permissions.RolesAssign)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Revoke(RevokeRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await revokeRole.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao revogar papel", result);
    }
}
