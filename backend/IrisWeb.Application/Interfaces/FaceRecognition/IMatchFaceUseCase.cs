using FluentResults;
using IrisWeb.Application.Communication.FaceRecognition.Requests;
using IrisWeb.Application.Communication.FaceRecognition.Responses;

namespace IrisWeb.Application.Interfaces.FaceRecognition;

public interface IMatchFaceUseCase
{
    Task<Result<MatchFaceResponse>> ExecuteAsync(
        MatchFaceRequest request,
        CancellationToken cancellationToken);
}
