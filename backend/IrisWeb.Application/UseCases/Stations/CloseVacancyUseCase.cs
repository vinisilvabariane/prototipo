using FluentResults;
using IrisWeb.Application.Communication.Stations.Requests;
using IrisWeb.Application.Interfaces.Stations;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.Stations;

public sealed class CloseVacancyUseCase(IStationVacancyRepository vacancies) : ICloseVacancyUseCase
{
    public async Task<Result> ExecuteAsync(
        long vacancyId,
        CloseVacancyRequest request,
        CancellationToken cancellationToken)
    {
        if (vacancyId <= 0)
            return Result.Fail(StationErrors.InvalidSession);

        var vacancy = await vacancies.GetByIdAsync(vacancyId, cancellationToken);

        if (vacancy is null || !vacancy.Close(request.EndedSecondsAgo))
            return Result.Ok();

        vacancies.Update(vacancy);
        await vacancies.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
