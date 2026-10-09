using FluentResults;
using IrisWeb.Application.Communication.Matrix.Requests;
using IrisWeb.Application.Communication.Stations.Responses;
using IrisWeb.Application.Interfaces.Matrix;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.Matrix;

public sealed class OpenDeviceVacancyUseCase(
    IDeviceMatrixBindingRepository bindings,
    IStationVacancyRepository vacancies) : IOpenDeviceVacancyUseCase
{
    public async Task<Result<OpenVacancyResponse>> ExecuteAsync(
        string deviceId,
        OpenDeviceVacancyRequest request,
        CancellationToken cancellationToken)
    {
        if (MatrixMapping.DeviceIdViolation(deviceId) is { } violation)
            return Result.Fail(violation);

        var binding = await bindings.GetByIdAsync(deviceId.Trim(), cancellationToken);
        if (binding is null)
            return Result.Fail(MatrixErrors.DeviceNotBound);

        // Mesma razao da presenca: sem isso, uma vacancia que o app nao fechou (crash, processo
        // morto) fica "vazio" para sempre na Linha.
        foreach (var old in await vacancies.ListOpenAsync(binding.PresenceWorkstation, binding.PresenceLine, cancellationToken))
        {
            if (old.Close(0))
                vacancies.Update(old);
        }

        var vacancy = binding.OpenVacancy(request.StartedSecondsAgo);

        await vacancies.AddAsync(vacancy, cancellationToken);

        if (await vacancies.SaveChangesAsync(cancellationToken) <= 0)
            return Result.Fail(StationErrors.NothingPersisted);

        return Result.Ok(new OpenVacancyResponse(vacancy.Id, vacancy.Workstation, vacancy.Line));
    }
}
