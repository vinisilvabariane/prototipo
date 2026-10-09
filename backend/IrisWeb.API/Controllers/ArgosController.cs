using IrisWeb.API.Errors;
using IrisWeb.Application.Communication.Argos.Requests;
using IrisWeb.Application.Communication.Argos.Responses;
using IrisWeb.Application.Interfaces.Argos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IrisWeb.API.Controllers;

[ApiController]
[Route("api/v1/argos")]
public class ArgosController(
    IGetArgosSummaryUseCase getSummary,
    IListArgosCyclesUseCase listCycles,
    IListArgosFiltersUseCase listFilters) : ControllerBase
{
    // Anonimo de proposito: a TV da linha e tela de parede, sem login. So devolve agregados, sem
    // nome de pessoa. Os outros dois endpoints pedem token.
    [HttpGet("summary")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ArgosSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ArgosSummaryResponse>> Summary(
        [FromQuery] ArgosSummaryQuery query,
        CancellationToken cancellationToken)
    {
        var result = await getSummary.ExecuteAsync(query, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);

        return ApiResults.Failure("Não foi possível montar o resumo do Crono Argos", result);
    }

    [HttpGet("cycles")]
    [Authorize]
    [ProducesResponseType(typeof(ArgosCyclesResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ArgosCyclesResponse>> Cycles(
        [FromQuery] ArgosCyclesQuery query,
        CancellationToken cancellationToken)
    {
        var result = await listCycles.ExecuteAsync(query, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);

        return ApiResults.Failure("Não foi possível listar as peças do Crono Argos", result);
    }

    [HttpGet("filters")]
    [Authorize]
    [ProducesResponseType(typeof(ArgosFiltersResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ArgosFiltersResponse>> Filters(CancellationToken cancellationToken)
    {
        var result = await listFilters.ExecuteAsync(cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);

        return ApiResults.Failure("Não foi possível carregar os filtros do Crono Argos", result);
    }
}
