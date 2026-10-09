using FluentResults;
using IrisWeb.Application.Interfaces.Matrix;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.Matrix;

public sealed class UnbindDeviceMatrixUseCase(IDeviceMatrixBindingRepository bindings) : IUnbindDeviceMatrixUseCase
{
    public async Task<Result> ExecuteAsync(string deviceId, CancellationToken cancellationToken)
    {
        if (MatrixMapping.DeviceIdViolation(deviceId) is { } violation)
            return Result.Fail(violation);

        var binding = await bindings.GetByIdAsync(deviceId.Trim(), cancellationToken);
        if (binding is null)
            return Result.Ok();

        bindings.Delete(binding);
        await bindings.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
