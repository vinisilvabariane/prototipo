using IrisWeb.API.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IrisWeb.Application.Communication.Stations.Responses;
using IrisWeb.Application.Communication.Stations.Requests;
using IrisWeb.Application.Interfaces.Stations;

namespace IrisWeb.API.Controllers;

[ApiController]
[Route("api/v1/stations")]
public class StationsController(
    IListStationTimelineUseCase listTimeline,
    IClosePresenceUseCase closePresence,
    ICloseVacancyUseCase closeVacancy,
    ICloseUnknownUseCase closeUnknown,
    IGetLineTimelineUseCase lineTimeline,
    IExportStationRecordsUseCase exportRecords) : ControllerBase
{
    private const string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    [HttpGet("export")]
    [Authorize]
    [Produces(XlsxContentType)]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> Export(
        [FromQuery] StationTimelineQuery query,
        CancellationToken cancellationToken)
    {
        var result = await exportRecords.ExecuteAsync(query, cancellationToken);

        if (result.IsSuccess)
            return File(result.Value.Content, XlsxContentType, result.Value.FileName);

        return ApiResults.Failure("Não foi possível exportar os registros", result);
    }

    [HttpGet("line-timeline")]
    [Authorize]
    [ProducesResponseType(typeof(LineTimelineResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LineTimelineResponse>> LineTimeline(
        [FromQuery] LineTimelineQuery query,
        CancellationToken cancellationToken)
    {
        var result = await lineTimeline.ExecuteAsync(query, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);

        return ApiResults.Failure("Não foi possível montar a linha do tempo da linha", result);
    }

    [HttpPost("unknown/{unknownId:long}/close")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> CloseUnknown(
        long unknownId,
        CloseUnknownRequest request,
        CancellationToken cancellationToken)
    {
        var result = await closeUnknown.ExecuteAsync(unknownId, request, cancellationToken);

        if (result.IsSuccess)
            return NoContent();

        return ApiResults.Failure("Não foi possível fechar o período de rosto desconhecido", result);
    }

    [HttpGet("timeline")]
    [Authorize]
    [ProducesResponseType(typeof(StationTimelineResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<StationTimelineResponse>> Timeline(
        [FromQuery] StationTimelineQuery query,
        CancellationToken cancellationToken)
    {
        var result = await listTimeline.ExecuteAsync(query, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);

        return ApiResults.Failure("Não foi possível consultar o histórico do posto", result);
    }

    [HttpPost("presence/{sessionId:long}/close")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> ClosePresence(
        long sessionId,
        ClosePresenceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await closePresence.ExecuteAsync(sessionId, request, cancellationToken);

        if (result.IsSuccess)
            return NoContent();

        return ApiResults.Failure("Não foi possível fechar a sessão de presença", result);
    }

    [HttpPost("vacancy/{vacancyId:long}/close")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> CloseVacancy(
        long vacancyId,
        CloseVacancyRequest request,
        CancellationToken cancellationToken)
    {
        var result = await closeVacancy.ExecuteAsync(vacancyId, request, cancellationToken);

        if (result.IsSuccess)
            return NoContent();

        return ApiResults.Failure("Não foi possível fechar o período de posto vazio", result);
    }
}
