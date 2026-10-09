using FluentResults;
using IrisWeb.Application.Communication.Matrix.Requests;
using IrisWeb.Application.Communication.Stations.Responses;

namespace IrisWeb.Application.Interfaces.Matrix;

public interface IOpenDevicePresenceUseCase
{
    Task<Result<OpenPresenceResponse>> ExecuteAsync(
        string deviceId,
        OpenDevicePresenceRequest request,
        CancellationToken cancellationToken);
}
