using FluentResults;
using IrisWeb.Application.Communication.Matrix.Requests;
using IrisWeb.Application.Communication.Stations.Responses;

namespace IrisWeb.Application.Interfaces.Matrix;

public interface IOpenDeviceUnknownUseCase
{
    Task<Result<OpenUnknownResponse>> ExecuteAsync(
        string deviceId,
        OpenDeviceUnknownRequest request,
        CancellationToken cancellationToken);
}
