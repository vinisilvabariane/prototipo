using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Tests.Fakes;

public sealed class FakeStationPresenceRepository
    : InMemoryRepository<StationPresence, long>, IStationPresenceRepository
{
    public Task<IReadOnlyList<StationPresence>> ListOverlappingAsync(
        DateTime from,
        DateTime to,
        string? workstation,
        string? line,
        string? registration,
        int limit,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<StationPresence> found = [.. Items
            .Where(presence => presence.EnteredAt < to)
            .Where(presence => presence.LeftAt is null || presence.LeftAt > from)
            .Where(presence => workstation is null || presence.Workstation == workstation)
            .Where(presence => line is null || presence.Line == line)
            .Where(presence => registration is null || presence.Registration == registration)
            .OrderByDescending(presence => presence.EnteredAt)
            .Take(limit)];

        return Task.FromResult(found);
    }

    public Task<IReadOnlyList<StationPresence>> ListOpenAsync(
        string workstation,
        string line,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<StationPresence> found = [.. Items
            .Where(presence => presence.Workstation == workstation)
            .Where(presence => presence.Line == line)
            .Where(presence => presence.LeftAt is null)];

        return Task.FromResult(found);
    }

    public Task<IReadOnlyList<StationPresence>> ListOpenOlderThanAsync(
        DateTime cutoff,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<StationPresence> found = [.. Items
            .Where(presence => presence.LeftAt is null)
            .Where(presence => presence.EnteredAt < cutoff)];

        return Task.FromResult(found);
    }
}
