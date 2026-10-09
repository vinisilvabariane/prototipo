using FluentResults;
using IrisWeb.Application.Communication.Matrix.Responses;
using IrisWeb.Application.Interfaces.Matrix;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.Matrix;

public sealed class GetDeviceMatrixUseCase(IDeviceMatrixBindingRepository bindings) : IGetDeviceMatrixUseCase
{
    public async Task<Result<DeviceMatrixResponse?>> ExecuteAsync(string deviceId, CancellationToken cancellationToken)
    {
        if (MatrixMapping.DeviceIdViolation(deviceId) is { } violation)
            return Result.Fail(violation);

        var binding = await bindings.GetByIdAsync(deviceId.Trim(), cancellationToken);

        return Result.Ok(binding is null ? null : MatrixMapping.ToResponse(binding));
    }
}
