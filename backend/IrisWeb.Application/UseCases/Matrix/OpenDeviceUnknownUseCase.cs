using FluentResults;
using IrisWeb.Application.Communication.Matrix.Requests;
using IrisWeb.Application.Communication.Stations.Responses;
using IrisWeb.Application.Interfaces.Matrix;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.Matrix;

public sealed class OpenDeviceUnknownUseCase(
    IDeviceMatrixBindingRepository bindings,
    IStationUnknownRepository unknowns) : IOpenDeviceUnknownUseCase
{
    public async Task<Result<OpenUnknownResponse>> ExecuteAsync(
        string deviceId,
        OpenDeviceUnknownRequest request,
        CancellationToken cancellationToken)
    {
        if (MatrixMapping.DeviceIdViolation(deviceId) is { } violation)
            return Result.Fail(violation);

        var binding = await bindings.GetByIdAsync(deviceId.Trim(), cancellationToken);
        if (binding is null)
            return Result.Fail(MatrixErrors.DeviceNotBound);

        // Mesma razao da presenca: sem isso, um "rosto desconhecido" que o app nao fechou (crash,
        // processo morto) fica aberto para sempre na Linha.
        foreach (var old in await unknowns.ListOpenAsync(binding.PresenceWorkstation, binding.PresenceLine, cancellationToken))
        {
            if (old.Close(0, null, null))
                unknowns.Update(old);
        }

        var unknown = binding.OpenUnknown(request.StartedSecondsAgo);

        await unknowns.AddAsync(unknown, cancellationToken);

        if (await unknowns.SaveChangesAsync(cancellationToken) <= 0)
            return Result.Fail(StationErrors.NothingPersisted);

        return Result.Ok(new OpenUnknownResponse(unknown.Id, unknown.Workstation, unknown.Line));
    }
}
