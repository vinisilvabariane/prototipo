using MeOne.API.Auth;
using MeOne.API.Errors;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Communication.Production.Responses;
using MeOne.Application.Interfaces.Production;
using MeOne.Domain.Rules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeOne.API.Controllers;

[ApiController]
[Route("api/v1/production/layouts")]
public class ProductionLayoutsController(
    IListLayoutsUseCase listLayouts,
    IGetLayoutUseCase getLayout,
    ICreateLayoutUseCase createLayout,
    ISaveLayoutUseCase saveLayout,
    IDeleteLayoutUseCase deleteLayout,
    ICopyLayoutUseCase copyLayout) : ControllerBase
{
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(LayoutListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<LayoutListResponse>> List([FromQuery] LayoutsQuery query, CancellationToken cancellationToken)
    {
        var result = await listLayouts.ExecuteAsync(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Erro ao buscar layouts", result);
    }

    [HttpGet("{id:long}")]
    [Authorize]
    [ProducesResponseType(typeof(LayoutDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LayoutDetailResponse>> GetById(ulong id, CancellationToken cancellationToken)
    {
        var result = await getLayout.ExecuteAsync(id, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Falha ao buscar layout", result);
    }

    [HttpPost]
    [RequirePermission(Permissions.EngineeringManage)]
    [ProducesResponseType(typeof(LayoutSummaryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LayoutSummaryResponse>> Create([FromBody] CreateLayoutRequest request, CancellationToken cancellationToken)
    {
        var result = await createLayout.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : ApiResults.Failure("Falha ao criar layout", result);
    }

    [HttpPut("{id:long}")]
    [RequirePermission(Permissions.EngineeringManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Save(ulong id, [FromBody] SaveLayoutRequest request, CancellationToken cancellationToken)
    {
        var result = await saveLayout.ExecuteAsync(id, request, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao salvar layout", result);
    }

    [HttpDelete("{id:long}")]
    [RequirePermission(Permissions.EngineeringManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(ulong id, CancellationToken cancellationToken)
    {
        var result = await deleteLayout.ExecuteAsync(id, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao excluir layout", result);
    }

    [HttpPost("{id:long}/copy")]
    [RequirePermission(Permissions.EngineeringManage)]
    [ProducesResponseType(typeof(CopyLayoutResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CopyLayoutResponse>> Copy(ulong id, [FromBody] CopyLayoutRequest request, CancellationToken cancellationToken)
    {
        var result = await copyLayout.ExecuteAsync(id, request, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Falha ao copiar layout", result);
    }
}
