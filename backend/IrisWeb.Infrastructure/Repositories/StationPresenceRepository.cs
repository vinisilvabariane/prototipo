using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Interfaces;
using IrisWeb.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace IrisWeb.Infrastructure.Repositories;

public sealed class StationPresenceRepository(IrisWebDbContext context)
    : Repository<StationPresence, long>(context), IStationPresenceRepository
{
    public async Task<IReadOnlyList<StationPresence>> ListOverlappingAsync(
        DateTime from,
        DateTime to,
        string? workstation,
        string? line,
        string? registration,
        int limit,
        CancellationToken cancellationToken)
    {
        var query = DbSet
            .AsNoTracking()
            .Where(presence => presence.EnteredAt < to)
            .Where(presence => presence.LeftAt == null || presence.LeftAt > from);

        if (!string.IsNullOrWhiteSpace(workstation))
            query = query.Where(presence => presence.Workstation == workstation);

        if (!string.IsNullOrWhiteSpace(line))
            query = query.Where(presence => presence.Line == line);

        if (!string.IsNullOrWhiteSpace(registration))
            query = query.Where(presence => presence.Registration == registration);

        return await query
            .OrderByDescending(presence => presence.EnteredAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StationPresence>> ListOpenAsync(
        string workstation,
        string line,
        CancellationToken cancellationToken) =>
        await DbSet
            .Where(presence => presence.Workstation == workstation)
            .Where(presence => presence.Line == line)
            .Where(presence => presence.LeftAt == null)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<StationPresence>> ListOpenOlderThanAsync(
        DateTime cutoff,
        CancellationToken cancellationToken) =>
        await DbSet
            .Where(presence => presence.LeftAt == null)
            .Where(presence => presence.EnteredAt < cutoff)
            .ToListAsync(cancellationToken);
}
