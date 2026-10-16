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
[Route("api/v1/production")]
public class ProductionSetupController(
    IListProductionLinesUseCase listLines,
    ICreateProductionLineUseCase createLine,
    IUpdateProductionLineUseCase updateLine,
    IListProductionShiftsUseCase listShifts,
    ICreateProductionShiftUseCase createShift,
    IUpdateProductionShiftUseCase updateShift,
    IListProductionProductsUseCase listProducts,
    ICreateProductionProductUseCase createProduct,
    IUpdateProductionProductUseCase updateProduct) : ControllerBase
{
    [HttpGet("lines")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<ProductionLineResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductionLineResponse>>> Lines([FromQuery] SetupQuery query, CancellationToken cancellationToken)
    {
        var result = await listLines.ExecuteAsync(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Erro ao buscar linhas", result);
    }

    [HttpPost("lines")]
    [RequirePermission(Permissions.EngineeringManage)]
    [ProducesResponseType(typeof(ProductionLineResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductionLineResponse>> CreateLine([FromBody] SaveProductionLineRequest request, CancellationToken cancellationToken)
    {
        var result = await createLine.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : ApiResults.Failure("Falha ao cadastrar linha", result);
    }

    [HttpPut("lines/{id:long}")]
    [RequirePermission(Permissions.EngineeringManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateLine(ulong id, [FromBody] SaveProductionLineRequest request, CancellationToken cancellationToken)
    {
        var result = await updateLine.ExecuteAsync(id, request, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao atualizar linha", result);
    }

    [HttpGet("shifts")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<ProductionShiftResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductionShiftResponse>>> Shifts([FromQuery] SetupQuery query, CancellationToken cancellationToken)
    {
        var result = await listShifts.ExecuteAsync(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Erro ao buscar turnos", result);
    }

    [HttpPost("shifts")]
    [RequirePermission(Permissions.EngineeringManage)]
    [ProducesResponseType(typeof(ProductionShiftResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductionShiftResponse>> CreateShift([FromBody] SaveProductionShiftRequest request, CancellationToken cancellationToken)
    {
        var result = await createShift.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : ApiResults.Failure("Falha ao cadastrar turno", result);
    }

    [HttpPut("shifts/{id:long}")]
    [RequirePermission(Permissions.EngineeringManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateShift(ulong id, [FromBody] SaveProductionShiftRequest request, CancellationToken cancellationToken)
    {
        var result = await updateShift.ExecuteAsync(id, request, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao atualizar turno", result);
    }

    [HttpGet("products")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<ProductionProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductionProductResponse>>> Products([FromQuery] SetupQuery query, CancellationToken cancellationToken)
    {
        var result = await listProducts.ExecuteAsync(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Erro ao buscar produtos", result);
    }

    [HttpPost("products")]
    [RequirePermission(Permissions.EngineeringManage)]
    [ProducesResponseType(typeof(ProductionProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProductionProductResponse>> CreateProduct([FromBody] SaveProductionProductRequest request, CancellationToken cancellationToken)
    {
        var result = await createProduct.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : ApiResults.Failure("Falha ao cadastrar produto", result);
    }

    [HttpPut("products/{id:long}")]
    [RequirePermission(Permissions.EngineeringManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(ulong id, [FromBody] SaveProductionProductRequest request, CancellationToken cancellationToken)
    {
        var result = await updateProduct.ExecuteAsync(id, request, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao atualizar produto", result);
    }
}
