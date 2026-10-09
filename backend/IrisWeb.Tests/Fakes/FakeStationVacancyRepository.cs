using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Tests.Fakes;

public sealed class FakeStationVacancyRepository
    : InMemoryRepository<StationVacancy, long>, IStationVacancyRepository
{
    public Task<IReadOnlyList<StationVacancy>> ListOverlappingAsync(
        DateTime from,
        DateTime to,
        string? workstation,
        string? line,
        int limit,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<StationVacancy> found = [.. Items
            .Where(vacancy => vacancy.StartedAt < to)
            .Where(vacancy => vacancy.EndedAt is null || vacancy.EndedAt > from)
            .Where(vacancy => workstation is null || vacancy.Workstation == workstation)
            .Where(vacancy => line is null || vacancy.Line == line)
            .OrderByDescending(vacancy => vacancy.StartedAt)
            .Take(limit)];

        return Task.FromResult(found);
    }

    public Task<IReadOnlyList<StationVacancy>> ListOpenAsync(
        string workstation,
        string line,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<StationVacancy> found = [.. Items
            .Where(vacancy => vacancy.Workstation == workstation)
            .Where(vacancy => vacancy.Line == line)
            .Where(vacancy => vacancy.EndedAt is null)];

        return Task.FromResult(found);
    }

    public Task<IReadOnlyList<StationVacancy>> ListOpenOlderThanAsync(
        DateTime cutoff,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<StationVacancy> found = [.. Items
            .Where(vacancy => vacancy.EndedAt is null)
            .Where(vacancy => vacancy.StartedAt < cutoff)];

        return Task.FromResult(found);
    }
}
