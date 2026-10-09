using IrisWeb.API.Auth;
using IrisWeb.API.Errors;
using IrisWeb.Application.Communication.Common.Responses;
using IrisWeb.Application.Communication.Roles.Requests;
using IrisWeb.Application.Communication.Roles.Responses;
using IrisWeb.Application.Interfaces.Roles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IrisWeb.API.Controllers;

/// <summary>
/// Atribuir e revogar role era aberto a qualquer um no Access Control; aqui é master, como o resto
/// da escrita.
/// </summary>
[ApiController]
[Route("api/v1/roles")]
public class RolesController(
    IListRolesUseCase listRoles,
    IListAllRolesUseCase listAllRoles,
    IGetRoleUseCase getRole,
    ICreateRoleUseCase createRole,
    IUpdateRoleUseCase updateRole,
    IDeleteRoleUseCase deleteRole,
    IAssignRoleUseCase assignRole,
    IRevokeRoleUseCase revokeRole) : ControllerBase
{
    /// <summary>Página começa em 0 (no Access Control começava em 1 só aqui).</summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(PagedResponse<RoleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<RoleResponse>>> List(
        [FromQuery] RolesQuery query,
        CancellationToken cancellationToken)
    {
        var result = await listRoles.ExecuteAsync(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Erro ao buscar roles", result);
    }

    [HttpGet("all")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<RoleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RoleResponse>>> All(CancellationToken cancellationToken)
    {
        var result = await listAllRoles.ExecuteAsync(cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Erro ao buscar roles", result);
    }

    [HttpGet("{id:long}")]
    [Authorize]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoleResponse>> GetById(ulong id, CancellationToken cancellationToken)
    {
        var result = await getRole.ExecuteAsync(id, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Falha ao buscar role", result);
    }

    [HttpPost]
    [Authorize(Policy = AuthPolicies.MasterOnly)]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<RoleResponse>> Create(CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await createRole.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : ApiResults.Failure("Falha ao criar role", result);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = AuthPolicies.MasterOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(ulong id, UpdateRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await updateRole.ExecuteAsync(id, request, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao atualizar role", result);
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = AuthPolicies.MasterOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(ulong id, CancellationToken cancellationToken)
    {
        var result = await deleteRole.ExecuteAsync(id, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao excluir role", result);
    }

    [HttpPost("set-role")]
    [Authorize(Policy = AuthPolicies.MasterOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Assign(AssignRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await assignRole.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao atribuir role", result);
    }

    [HttpPost("revoke-role")]
    [Authorize(Policy = AuthPolicies.MasterOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Revoke(RevokeRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await revokeRole.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao revogar role", result);
    }
}
