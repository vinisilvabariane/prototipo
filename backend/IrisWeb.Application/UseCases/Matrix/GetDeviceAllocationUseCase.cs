using FluentResults;
using IrisWeb.Application.Communication.Matrix.Responses;
using IrisWeb.Application.Interfaces.Matrix;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.Matrix;

public sealed class GetDeviceAllocationUseCase(
    IDeviceMatrixBindingRepository bindings,
    ISyncMatrixClient sync) : IGetDeviceAllocationUseCase
{
    public async Task<Result<DeviceAllocationResponse>> ExecuteAsync(string deviceId, CancellationToken cancellationToken)
    {
        if (MatrixMapping.DeviceIdViolation(deviceId) is { } violation)
            return Result.Fail(violation);

        var binding = await bindings.GetByIdAsync(deviceId.Trim(), cancellationToken);
        if (binding is null)
            return Result.Fail(MatrixErrors.DeviceNotBound);

        var layout = await MatrixMapping.GuardAsync(() =>
            sync.GetLayoutAsync(binding.Line, binding.Product, binding.Shift, cancellationToken));

        if (layout.IsFailed)
            return layout.ToResult();

        var station = MatrixMapping.FindStation(layout.Value, binding.StationId);
        if (station is null)
            return Result.Fail(MatrixErrors.StationNotInLayout(binding.StationId, binding.Line, binding.Product, binding.Shift));

        return Result.Ok(new DeviceAllocationResponse(
            MatrixMapping.ToResponse(binding),
            MatrixMapping.AllocatedOn(station),
            DateTime.Now));
    }
}
