using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Enums;
using IrisWeb.Domain.Interfaces;
using IrisWeb.Domain.Rules;
using IrisWeb.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace IrisWeb.Infrastructure.Repositories;

public sealed class ArgosCycleRepository(IrisWebDbContext context)
    : Repository<ArgosCycle, long>(context), IArgosCycleRepository
{
    public async Task<IReadOnlyList<ArgosCycle>> ListAsync(
        ArgosCycleFilter filter,
        CancellationToken cancellationToken) =>
        await Apply(DbSet.AsNoTracking(), filter)
            .OrderBy(cycle => cycle.RecordedAt)
            .ToListAsync(cancellationToken);

    public async Task<(IReadOnlyList<ArgosCycle> Entries, int Total, int Page)> PageAsync(
        ArgosCycleFilter filter,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = Apply(DbSet.AsNoTracking(), filter);

        var total = await query.CountAsync(cancellationToken);
        var totalPages = total == 0 ? 1 : (int)Math.Ceiling(total / (double)pageSize);
        var effectivePage = Math.Clamp(page, 1, totalPages);

        var entries = await query
            .OrderByDescending(cycle => cycle.RecordedAt)
            .ThenByDescending(cycle => cycle.Id)
            .Skip((effectivePage - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (entries, total, effectivePage);
    }

    public async Task<DateTime?> LastRecordedAtAsync(
        string? station,
        string? setup,
        CancellationToken cancellationToken)
    {
        var query = DbSet.AsNoTracking();

        if (station is not null)
            query = query.Where(cycle => cycle.Station == station);

        if (setup is not null)
            query = query.Where(cycle => cycle.SetupName == setup);

        return await query
            .OrderByDescending(cycle => cycle.RecordedAt)
            .Select(cycle => (DateTime?)cycle.RecordedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<string>> ListStationsAsync(DateTime since, CancellationToken cancellationToken) =>
        await DbSet.AsNoTracking()
            .Where(cycle => cycle.RecordedAt >= since && cycle.Station != null)
            .Select(cycle => cycle.Station!)
            .Distinct()
            .OrderBy(station => station)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<string>> ListSetupsAsync(DateTime since, CancellationToken cancellationToken) =>
        await DbSet.AsNoTracking()
            .Where(cycle => cycle.RecordedAt >= since && cycle.SetupName != null)
            .Select(cycle => cycle.SetupName!)
            .Distinct()
            .OrderBy(setup => setup)
            .ToListAsync(cancellationToken);

    /// <summary>Aplica o filtro inteiro em SQL. Publico para o teste conferir o SQL gerado sem banco.</summary>
    public static IQueryable<ArgosCycle> Apply(IQueryable<ArgosCycle> query, ArgosCycleFilter filter)
    {
        var from = filter.From;
        var to = filter.To;

        query = query.Where(cycle => cycle.RecordedAt >= from && cycle.RecordedAt < to);

        query = filter.Result switch
        {
            ArgosResultFilter.Ok => query.Where(cycle => cycle.Status == 1),
            ArgosResultFilter.Ng => query.Where(cycle => cycle.Status != 1),
            _ => query,
        };

        if (filter.Station is { } station)
            query = query.Where(cycle => cycle.Station == station);

        if (filter.Setup is { } setup)
            query = query.Where(cycle => cycle.SetupName == setup);

        var admStart = ArgosShiftRule.AdmStart;
        var admEnd = ArgosShiftRule.AdmEnd;
        var t2Start = ArgosShiftRule.T2Start;
        var t2End = ArgosShiftRule.T2End;

        return filter.Shift switch
        {
            ArgosShiftFilter.Adm => query.Where(cycle =>
                cycle.RecordedAt.TimeOfDay >= admStart && cycle.RecordedAt.TimeOfDay < admEnd),
            ArgosShiftFilter.T2 => query.Where(cycle =>
                cycle.RecordedAt.TimeOfDay >= t2Start || cycle.RecordedAt.TimeOfDay < t2End),
            _ => query,
        };
    }
}
