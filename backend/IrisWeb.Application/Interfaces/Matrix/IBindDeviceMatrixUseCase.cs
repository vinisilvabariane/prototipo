using FluentResults;
using IrisWeb.Application.Communication.Matrix.Requests;
using IrisWeb.Application.Communication.Matrix.Responses;

namespace IrisWeb.Application.Interfaces.Matrix;

public interface IBindDeviceMatrixUseCase
{
    Task<Result<DeviceMatrixResponse>> ExecuteAsync(
        string deviceId,
        BindDeviceMatrixRequest request,
        CancellationToken cancellationToken);
}
