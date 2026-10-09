using FluentResults;
using IrisWeb.Application.Communication.Matrix.Responses;

namespace IrisWeb.Application.Interfaces.Matrix;

public interface IGetDeviceMatrixUseCase
{
    Task<Result<DeviceMatrixResponse?>> ExecuteAsync(string deviceId, CancellationToken cancellationToken);
}
