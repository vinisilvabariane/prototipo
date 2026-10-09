using IrisWeb.API.Errors;
using IrisWeb.Application.Communication.Matrix.Requests;
using IrisWeb.Application.Communication.Matrix.Responses;
using IrisWeb.Application.Communication.Stations.Responses;
using IrisWeb.Application.Interfaces.Matrix;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IrisWeb.API.Controllers;

[ApiController]
[Route("api/v1/matrix")]
public class MatrixController(
    IListMatrixLinesUseCase listLines,
    IListMatrixProductsUseCase listProducts,
    IListMatrixStationsUseCase listStations,
    IGetDeviceMatrixUseCase getDevice,
    IBindDeviceMatrixUseCase bindDevice,
    IUnbindDeviceMatrixUseCase unbindDevice,
    IGetDeviceAllocationUseCase getAllocation,
    IAllocateMatrixStationUseCase allocateStation,
    IOpenDevicePresenceUseCase openPresence,
    IOpenDeviceVacancyUseCase openVacancy,
    IOpenDeviceUnknownUseCase openUnknown,
    IListStationRecommendationsUseCase listRecommendations,
    ILookupStationCandidateUseCase lookupCandidate,
    IReleaseMatrixStationUseCase releaseStation) : ControllerBase
{
    [HttpPost("devices/{deviceId}/unknown")]
    [Authorize]
    [ProducesResponseType(typeof(OpenUnknownResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OpenUnknownResponse>> OpenUnknown(
        string deviceId,
        OpenDeviceUnknownRequest request,
        CancellationToken cancellationToken)
    {
        var result = await openUnknown.ExecuteAsync(deviceId, request, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);

        return ApiResults.Failure("Não foi possível abrir o período de rosto desconhecido", result);
    }

    [HttpDelete("stations/{stationId:int}/allocation")]
    [Authorize]
    [ProducesResponseType(typeof(MatrixStationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<MatrixStationResponse>> Release(
        int stationId,
        [FromQuery] MatrixStationsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await releaseStation.ExecuteAsync(stationId, query, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);

        return ApiResults.Failure("Não foi possível liberar o posto", result);
    }

    [HttpGet("stations/{stationId:int}/recommendations")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<MatrixCandidateResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<MatrixCandidateResponse>>> Recommendations(
        int stationId,
        [FromQuery] MatrixStationsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await listRecommendations.ExecuteAsync(stationId, query, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);

        return ApiResults.Failure("Não foi possível buscar os recomendados do posto", result);
    }

    [HttpGet("stations/{stationId:int}/candidates/{registration:int}")]
    [Authorize]
    [ProducesResponseType(typeof(MatrixCandidateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<MatrixCandidateResponse>> Candidate(
        int stationId,
        int registration,
        [FromQuery] MatrixStationsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await lookupCandidate.ExecuteAsync(stationId, registration, query, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);

        return ApiResults.Failure("Não foi possível buscar o operador", result);
    }

    [HttpPost("devices/{deviceId}/presence")]
    [Authorize]
    [ProducesResponseType(typeof(OpenPresenceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OpenPresenceResponse>> OpenPresence(
        string deviceId,
        OpenDevicePresenceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await openPresence.ExecuteAsync(deviceId, request, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);

        return ApiResults.Failure("Não foi possível abrir a sessão de presença", result);
    }

    [HttpPost("devices/{deviceId}/vacancy")]
    [Authorize]
    [ProducesResponseType(typeof(OpenVacancyResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<OpenVacancyResponse>> OpenVacancy(
        string deviceId,
        OpenDeviceVacancyRequest request,
        CancellationToken cancellationToken)
    {
        var result = await openVacancy.ExecuteAsync(deviceId, request, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);

        return ApiResults.Failure("Não foi possível abrir o período de posto vazio", result);
    }

    [HttpPost("allocations")]
    [Authorize]
    [ProducesResponseType(typeof(MatrixStationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<MatrixStationResponse>> Allocate(
        AllocateMatrixStationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await allocateStation.ExecuteAsync(request, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);

        return ApiResults.Failure("Não foi possível alocar o colaborador no posto", result);
    }

    [HttpGet("lines")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<string>>> Lines(CancellationToken cancellationToken)
    {
        var result = await listLines.ExecuteAsync(cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);

        return ApiResults.Failure("Não foi possível listar as linhas do Sync", result);
    }

    [HttpGet("products")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<MatrixProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<MatrixProductResponse>>> Products(
        [FromQuery] MatrixProductsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await listProducts.ExecuteAsync(query, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);

        return ApiResults.Failure("Não foi possível listar os produtos da linha", result);
    }

    [HttpGet("stations")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<MatrixStationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<MatrixStationResponse>>> Stations(
        [FromQuery] MatrixStationsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await listStations.ExecuteAsync(query, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);

        return ApiResults.Failure("Não foi possível listar os postos do layout", result);
    }

    [HttpGet("devices/{deviceId}")]
    [Authorize]
    [ProducesResponseType(typeof(DeviceMatrixResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<DeviceMatrixResponse>> Device(string deviceId, CancellationToken cancellationToken)
    {
        var result = await getDevice.ExecuteAsync(deviceId, cancellationToken);

        if (result.IsFailed)
            return ApiResults.Failure("Não foi possível consultar o vínculo do aparelho", result);

        return result.Value is null ? NoContent() : Ok(result.Value);
    }

    [HttpPut("devices/{deviceId}")]
    [Authorize]
    [ProducesResponseType(typeof(DeviceMatrixResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<DeviceMatrixResponse>> Bind(
        string deviceId,
        BindDeviceMatrixRequest request,
        CancellationToken cancellationToken)
    {
        var result = await bindDevice.ExecuteAsync(deviceId, request, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);

        return ApiResults.Failure("Não foi possível vincular o aparelho ao posto", result);
    }

    [HttpDelete("devices/{deviceId}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> Unbind(string deviceId, CancellationToken cancellationToken)
    {
        var result = await unbindDevice.ExecuteAsync(deviceId, cancellationToken);

        if (result.IsSuccess)
            return NoContent();

        return ApiResults.Failure("Não foi possível desvincular o aparelho", result);
    }

    [HttpGet("devices/{deviceId}/allocation")]
    [Authorize]
    [ProducesResponseType(typeof(DeviceAllocationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<DeviceAllocationResponse>> Allocation(
        string deviceId,
        CancellationToken cancellationToken)
    {
        var result = await getAllocation.ExecuteAsync(deviceId, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);

        return ApiResults.Failure("Não foi possível consultar a alocação do posto", result);
    }
}
