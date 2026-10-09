using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Tests.Fakes;

public sealed class FakeStationUnknownRepository
    : InMemoryRepository<StationUnknown, long>, IStationUnknownRepository
{
    public Task<IReadOnlyList<StationUnknown>> ListOverlappingAsync(
        DateTime from,
        DateTime to,
        string? workstation,
        string? line,
        int limit,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<StationUnknown> found = [.. Items
            .Where(unknown => unknown.StartedAt < to)
            .Where(unknown => unknown.EndedAt is null || unknown.EndedAt > from)
            .Where(unknown => workstation is null || unknown.Workstation == workstation)
            .Where(unknown => line is null || unknown.Line == line)
            .OrderByDescending(unknown => unknown.StartedAt)
            .Take(limit)];

        return Task.FromResult(found);
    }

    public Task<IReadOnlyList<StationUnknown>> ListOpenAsync(
        string workstation,
        string line,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<StationUnknown> found = [.. Items
            .Where(unknown => unknown.Workstation == workstation)
            .Where(unknown => unknown.Line == line)
            .Where(unknown => unknown.EndedAt is null)];

        return Task.FromResult(found);
    }

    public Task<IReadOnlyList<StationUnknown>> ListOpenOlderThanAsync(
        DateTime cutoff,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<StationUnknown> found = [.. Items
            .Where(unknown => unknown.EndedAt is null)
            .Where(unknown => unknown.StartedAt < cutoff)];

        return Task.FromResult(found);
    }
}
