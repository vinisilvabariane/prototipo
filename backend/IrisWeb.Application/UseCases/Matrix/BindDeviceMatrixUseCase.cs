using FluentResults;
using IrisWeb.Application.Communication.Matrix.Requests;
using IrisWeb.Application.Communication.Matrix.Responses;
using IrisWeb.Application.Interfaces.Matrix;
using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.Matrix;

public sealed class BindDeviceMatrixUseCase(
    IDeviceMatrixBindingRepository bindings,
    ISyncMatrixClient sync) : IBindDeviceMatrixUseCase
{
    public async Task<Result<DeviceMatrixResponse>> ExecuteAsync(
        string deviceId,
        BindDeviceMatrixRequest request,
        CancellationToken cancellationToken)
    {
        if (MatrixMapping.DeviceIdViolation(deviceId) is { } violation)
            return Result.Fail(violation);

        var line = request.Line.Trim();
        var product = request.Product.Trim();
        var shift = request.Shift.Trim();

        var layout = await MatrixMapping.GuardAsync(() => sync.GetLayoutAsync(line, product, shift, cancellationToken));

        if (layout.IsFailed)
            return layout.ToResult();

        var station = MatrixMapping.FindStation(layout.Value, request.StationId);
        if (station is null)
            return Result.Fail(MatrixErrors.StationNotInLayout(request.StationId, line, product, shift));

        var stationName = Truncate(station.Name, MatrixErrors.MaxStationNameLength);
        var binding = await bindings.GetByIdAsync(deviceId.Trim(), cancellationToken);

        if (binding is null)
        {
            binding = DeviceMatrixBinding.Create(deviceId, line, product, shift, station.Id, stationName);
            await bindings.AddAsync(binding, cancellationToken);
        }
        else
        {
            binding.Rebind(line, product, shift, station.Id, stationName);
            bindings.Update(binding);
        }

        if (await bindings.SaveChangesAsync(cancellationToken) <= 0)
            return Result.Fail(MatrixErrors.NothingPersisted);

        return Result.Ok(MatrixMapping.ToResponse(binding));
    }

    private static string Truncate(string? value, int max)
    {
        var trimmed = (value ?? string.Empty).Trim();
        return trimmed.Length <= max ? trimmed : trimmed[..max];
    }
}
