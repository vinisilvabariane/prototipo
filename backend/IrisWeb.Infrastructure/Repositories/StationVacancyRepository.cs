using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Interfaces;
using IrisWeb.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace IrisWeb.Infrastructure.Repositories;

public sealed class StationVacancyRepository(IrisWebDbContext context)
    : Repository<StationVacancy, long>(context), IStationVacancyRepository
{
    public async Task<IReadOnlyList<StationVacancy>> ListOverlappingAsync(
        DateTime from,
        DateTime to,
        string? workstation,
        string? line,
        int limit,
        CancellationToken cancellationToken)
    {
        var query = DbSet
            .AsNoTracking()
            .Where(vacancy => vacancy.StartedAt < to)
            .Where(vacancy => vacancy.EndedAt == null || vacancy.EndedAt > from);

        if (!string.IsNullOrWhiteSpace(workstation))
            query = query.Where(vacancy => vacancy.Workstation == workstation);

        if (!string.IsNullOrWhiteSpace(line))
            query = query.Where(vacancy => vacancy.Line == line);

        return await query
            .OrderByDescending(vacancy => vacancy.StartedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StationVacancy>> ListOpenAsync(
        string workstation,
        string line,
        CancellationToken cancellationToken) =>
        await DbSet
            .Where(vacancy => vacancy.Workstation == workstation)
            .Where(vacancy => vacancy.Line == line)
            .Where(vacancy => vacancy.EndedAt == null)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<StationVacancy>> ListOpenOlderThanAsync(
        DateTime cutoff,
        CancellationToken cancellationToken) =>
        await DbSet
            .Where(vacancy => vacancy.EndedAt == null)
            .Where(vacancy => vacancy.StartedAt < cutoff)
            .ToListAsync(cancellationToken);
}
