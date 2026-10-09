using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Interfaces;
using IrisWeb.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace IrisWeb.Infrastructure.Repositories;

public sealed class StationUnknownRepository(IrisWebDbContext context)
    : Repository<StationUnknown, long>(context), IStationUnknownRepository
{
    public async Task<IReadOnlyList<StationUnknown>> ListOverlappingAsync(
        DateTime from,
        DateTime to,
        string? workstation,
        string? line,
        int limit,
        CancellationToken cancellationToken)
    {
        var query = DbSet
            .AsNoTracking()
            .Where(unknown => unknown.StartedAt < to)
            .Where(unknown => unknown.EndedAt == null || unknown.EndedAt > from);

        if (!string.IsNullOrWhiteSpace(workstation))
            query = query.Where(unknown => unknown.Workstation == workstation);

        if (!string.IsNullOrWhiteSpace(line))
            query = query.Where(unknown => unknown.Line == line);

        return await query
            .OrderByDescending(unknown => unknown.StartedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StationUnknown>> ListOpenAsync(
        string workstation,
        string line,
        CancellationToken cancellationToken) =>
        await DbSet
            .Where(unknown => unknown.Workstation == workstation)
            .Where(unknown => unknown.Line == line)
            .Where(unknown => unknown.EndedAt == null)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<StationUnknown>> ListOpenOlderThanAsync(
        DateTime cutoff,
        CancellationToken cancellationToken) =>
        await DbSet
            .Where(unknown => unknown.EndedAt == null)
            .Where(unknown => unknown.StartedAt < cutoff)
            .ToListAsync(cancellationToken);
}
