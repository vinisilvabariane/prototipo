using FluentResults;
using IrisWeb.Application.Communication.FaceRecognition.Requests;
using IrisWeb.Application.Communication.FaceRecognition.Responses;

namespace IrisWeb.Application.Interfaces.FaceRecognition;

public interface IEnrollFaceBatchUseCase
{
    Task<Result<EnrollFaceBatchResponse>> ExecuteAsync(EnrollFaceBatchRequest request, CancellationToken cancellationToken);
}
