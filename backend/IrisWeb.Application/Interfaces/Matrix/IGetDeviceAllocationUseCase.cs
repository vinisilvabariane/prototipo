using FluentResults;
using IrisWeb.Application.Communication.Matrix.Responses;

namespace IrisWeb.Application.Interfaces.Matrix;

public interface IGetDeviceAllocationUseCase
{
    Task<Result<DeviceAllocationResponse>> ExecuteAsync(string deviceId, CancellationToken cancellationToken);
}
