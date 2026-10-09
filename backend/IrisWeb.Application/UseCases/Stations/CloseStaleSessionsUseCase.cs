using FluentResults;
using IrisWeb.Application.Interfaces.Stations;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.Stations;

public sealed class CloseStaleSessionsUseCase(
    IStationPresenceRepository presences,
    IStationVacancyRepository vacancies,
    IStationUnknownRepository unknowns) : ICloseStaleSessionsUseCase
{
    public async Task<Result<int>> ExecuteAsync(TimeSpan maxOpenAge, CancellationToken cancellationToken)
    {
        var cutoff = DateTime.Now - maxOpenAge;
        var closed = 0;

        foreach (var presence in await presences.ListOpenOlderThanAsync(cutoff, cancellationToken))
        {
            if (!presence.Close(0)) continue;
            presences.Update(presence);
            closed++;
        }

        foreach (var vacancy in await vacancies.ListOpenOlderThanAsync(cutoff, cancellationToken))
        {
            if (!vacancy.Close(0)) continue;
            vacancies.Update(vacancy);
            closed++;
        }

        foreach (var unknown in await unknowns.ListOpenOlderThanAsync(cutoff, cancellationToken))
        {
            if (!unknown.Close(0, null, null)) continue;
            unknowns.Update(unknown);
            closed++;
        }

        await presences.SaveChangesAsync(cancellationToken);
        await vacancies.SaveChangesAsync(cancellationToken);
        await unknowns.SaveChangesAsync(cancellationToken);

        return Result.Ok(closed);
    }
}
