using IrisWeb.API.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IrisWeb.Application.Communication.FaceRecognition.Responses;
using IrisWeb.Application.Communication.FaceRecognition.Requests;
using IrisWeb.Application.Interfaces.FaceRecognition;

namespace IrisWeb.API.Controllers;

[ApiController]
[Route("api/v1/face-recognition")]
public class FaceRecognitionController(
    IMatchFaceUseCase matchFace,
    IEnrollFaceUseCase enrollFace,
    IEnrollFaceBatchUseCase enrollFaceBatch) : ControllerBase
{
    [HttpPost("match")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(MatchFaceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MatchFaceResponse>> Match(
        MatchFaceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await matchFace.ExecuteAsync(request, cancellationToken);
        if (result.IsSuccess)
            return Ok(result.Value);
        return ApiResults.Failure("Não foi possível comparar a face", result);
    }

    [HttpPost("enroll")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> Enroll(
        EnrollFaceRequest request,
        CancellationToken cancellationToken)
    {
        var result = await enrollFace.ExecuteAsync(request, cancellationToken);

        if (result.IsSuccess)
            return NoContent();
        return ApiResults.Failure("Não foi possível cadastrar o rosto", result);
    }

    [HttpPost("enroll/batch")]
    [Authorize]
    [ProducesResponseType(typeof(EnrollFaceBatchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<EnrollFaceBatchResponse>> EnrollBatch(
        EnrollFaceBatchRequest request,
        CancellationToken cancellationToken)
    {
        var result = await enrollFaceBatch.ExecuteAsync(request, cancellationToken);

        if (result.IsSuccess)
            return Ok(result.Value);
        return ApiResults.Failure("Não foi possível cadastrar o rosto", result);
    }
}
