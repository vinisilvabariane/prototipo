using MeOne.API.Auth;
using MeOne.API.Errors;
using MeOne.Application.Communication.OrgChart.Requests;
using MeOne.Application.Communication.OrgChart.Responses;
using MeOne.Application.Interfaces.OrgChart;
using MeOne.Domain.Rules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeOne.API.Controllers;

[ApiController]
[Route("api/v1/org-chart")]
public class OrgChartController(
    IListOrgChartUseCase listOrgChart,
    IListOrgAssignableUsersUseCase listAssignableUsers,
    ICreateOrgPositionUseCase createPosition,
    IUpdateOrgPositionUseCase updatePosition,
    IAssignOrgPositionHolderUseCase assignHolder,
    IMoveOrgPositionUseCase movePosition,
    IDeleteOrgPositionUseCase deletePosition) : ControllerBase
{
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(OrgChartResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<OrgChartResponse>> List(CancellationToken cancellationToken)
    {
        var result = await listOrgChart.ExecuteAsync(cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Erro ao buscar o organograma", result);
    }

    [HttpGet("assignable-users")]
    [RequirePermission(Permissions.OrgChartManage)]
    [ProducesResponseType(typeof(IReadOnlyList<OrgUserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrgUserResponse>>> AssignableUsers(CancellationToken cancellationToken)
    {
        var result = await listAssignableUsers.ExecuteAsync(cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Erro ao buscar colaboradores", result);
    }

    [HttpPost("positions")]
    [RequirePermission(Permissions.OrgChartManage)]
    [ProducesResponseType(typeof(OrgPositionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrgPositionResponse>> Create(
        CreateOrgPositionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await createPosition.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : ApiResults.Failure("Falha ao criar cargo", result);
    }

    [HttpPut("positions/{id:long}")]
    [RequirePermission(Permissions.OrgChartManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(ulong id, UpdateOrgPositionRequest request, CancellationToken cancellationToken)
    {
        var result = await updatePosition.ExecuteAsync(id, request, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao atualizar cargo", result);
    }

    [HttpPut("positions/{id:long}/holder")]
    [RequirePermission(Permissions.OrgChartManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignHolder(
        ulong id,
        AssignOrgPositionHolderRequest request,
        CancellationToken cancellationToken)
    {
        var result = await assignHolder.ExecuteAsync(id, request, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao definir o ocupante do cargo", result);
    }

    [HttpPut("positions/{id:long}/parent")]
    [RequirePermission(Permissions.OrgChartManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Move(ulong id, MoveOrgPositionRequest request, CancellationToken cancellationToken)
    {
        var result = await movePosition.ExecuteAsync(id, request, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao mover cargo", result);
    }

    [HttpDelete("positions/{id:long}")]
    [RequirePermission(Permissions.OrgChartManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(ulong id, CancellationToken cancellationToken)
    {
        var result = await deletePosition.ExecuteAsync(id, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao excluir cargo", result);
    }
}
