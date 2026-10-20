using MeOne.API.Auth;
using MeOne.API.Errors;
using MeOne.Application.Communication.Oee.Requests;
using MeOne.Application.Communication.Oee.Responses;
using MeOne.Application.Interfaces.Oee;
using MeOne.Domain.Rules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeOne.API.Controllers;

[ApiController]
[Route("api/v1/oee")]
public class OeeController(
    IListOeeRunsUseCase listRuns,
    IGetRunOeeUseCase getRun,
    IGetOeeConfigUseCase getConfig,
    ICreateOeeFormulaSetUseCase createSet,
    IUpdateOeeFormulaSetUseCase updateSet,
    IDeleteOeeFormulaSetUseCase deleteSet,
    ISetActiveOeeFormulaSetUseCase setActive,
    ITestOeeFormulaUseCase testFormula) : ControllerBase
{
    [HttpGet("runs")]
    [Authorize]
    [ProducesResponseType(typeof(OeeComparisonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OeeComparisonResponse>> Runs([FromQuery] OeeRunsQuery query, CancellationToken cancellationToken)
    {
        var result = await listRuns.ExecuteAsync(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Erro ao buscar o OEE das produções", result);
    }

    [HttpGet("runs/{id:long}")]
    [Authorize]
    [ProducesResponseType(typeof(RunOeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RunOeeResponse>> RunById(ulong id, CancellationToken cancellationToken)
    {
        var result = await getRun.ExecuteAsync(id, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Falha ao calcular o OEE", result);
    }

    [HttpGet("config")]
    [Authorize]
    [ProducesResponseType(typeof(OeeConfigResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<OeeConfigResponse>> Config(CancellationToken cancellationToken)
    {
        var result = await getConfig.ExecuteAsync(cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Erro ao buscar a configuração do OEE", result);
    }

    [HttpPost("formulas")]
    [RequirePermission(Permissions.EngineeringManage)]
    [ProducesResponseType(typeof(OeeFormulaSetResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<OeeFormulaSetResponse>> CreateFormulaSet([FromBody] SaveOeeFormulaSetRequest request, CancellationToken cancellationToken)
    {
        var result = await createSet.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : ApiResults.Failure("Falha ao criar o conjunto de fórmulas", result);
    }

    [HttpPut("formulas/{id:long}")]
    [RequirePermission(Permissions.EngineeringManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateFormulaSet(ulong id, [FromBody] SaveOeeFormulaSetRequest request, CancellationToken cancellationToken)
    {
        var result = await updateSet.ExecuteAsync(id, request, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao salvar o conjunto de fórmulas", result);
    }

    [HttpDelete("formulas/{id:long}")]
    [RequirePermission(Permissions.EngineeringManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFormulaSet(ulong id, CancellationToken cancellationToken)
    {
        var result = await deleteSet.ExecuteAsync(id, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao excluir o conjunto de fórmulas", result);
    }

    [HttpPut("formulas/active")]
    [RequirePermission(Permissions.EngineeringManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetActiveFormulaSet([FromBody] SetActiveOeeFormulaSetRequest request, CancellationToken cancellationToken)
    {
        var result = await setActive.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao ativar o conjunto de fórmulas", result);
    }

    [HttpPost("formulas/test")]
    [Authorize]
    [ProducesResponseType(typeof(OeeFormulaTestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OeeFormulaTestResponse>> TestFormula([FromBody] TestOeeFormulaRequest request, CancellationToken cancellationToken)
    {
        var result = await testFormula.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Falha ao testar a fórmula", result);
    }
}
