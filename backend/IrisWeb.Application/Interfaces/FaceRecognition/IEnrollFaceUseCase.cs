using FluentResults;
using IrisWeb.Application.Communication.FaceRecognition.Requests;

namespace IrisWeb.Application.Interfaces.FaceRecognition;

public interface IEnrollFaceUseCase
{
    Task<Result> ExecuteAsync(EnrollFaceRequest request, CancellationToken cancellationToken);
}
