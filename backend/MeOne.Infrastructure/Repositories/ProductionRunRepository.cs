using MeOne.Domain.Entities;
using MeOne.Domain.Enums;
using MeOne.Domain.Interfaces;
using MeOne.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace MeOne.Infrastructure.Repositories;

public sealed class ProductionRunRepository(MeOneDbContext context)
    : Repository<ProductionRun, ulong>(context), IProductionRunRepository
{
    public override Task<ProductionRun?> GetByIdAsync(ulong id, CancellationToken cancellationToken) =>
        DbSet.Include(run => run.Allocations)
            .Include(run => run.Outputs)
            .AsSplitQuery()
            .FirstOrDefaultAsync(run => run.Id == id, cancellationToken);

    public override void Update(ProductionRun entity)
    {
        if (Context.Entry(entity).State == EntityState.Detached)
            DbSet.Update(entity);
    }

    public Task<bool> LineHasActiveRunAsync(ulong lineId, CancellationToken cancellationToken) =>
        DbSet.AnyAsync(run => run.LineId == lineId && run.Status != RunStatus.Closed, cancellationToken);

    public async Task<IReadOnlyDictionary<ulong, ulong>> FindActiveRunsOfUsersAsync(
        IReadOnlyCollection<ulong> userIds,
        ulong exceptRunId,
        CancellationToken cancellationToken)
    {
        if (userIds.Count == 0)
            return new Dictionary<ulong, ulong>();

        var rows = await (
            from allocation in Context.Set<RunAllocation>().AsNoTracking()
            join run in DbSet.AsNoTracking() on allocation.RunId equals run.Id
            where allocation.ReleasedAt == null
                && userIds.Contains(allocation.UserId)
                && run.Status != RunStatus.Closed
                && run.Id != exceptRunId
            select new { allocation.UserId, run.LineId })
            .ToListAsync(cancellationToken);

        return rows.GroupBy(row => row.UserId).ToDictionary(group => group.Key, group => group.First().LineId);
    }

    public Task<ProductionRun?> GetLastClosedOfLayoutAsync(ulong layoutId, CancellationToken cancellationToken) =>
        DbSet.AsNoTracking()
            .Include(run => run.Allocations)
            .Where(run => run.LayoutId == layoutId && run.Status == RunStatus.Closed)
            .OrderByDescending(run => run.EndedAt)
            .ThenByDescending(run => run.Id)
            .FirstOrDefaultAsync(cancellationToken);

    public Task<int> CountAsync(RunStatus? status, ulong? lineId, CancellationToken cancellationToken) =>
        Filtered(status, lineId).CountAsync(cancellationToken);

    public async Task<IReadOnlyList<ProductionRun>> SearchAsync(
        RunStatus? status,
        ulong? lineId,
        int skip,
        int take,
        CancellationToken cancellationToken) =>
        await Filtered(status, lineId)
            .Include(run => run.Allocations)
            .Include(run => run.Outputs)
            .AsSplitQuery()
            .OrderByDescending(run => run.CreatedAt)
            .ThenByDescending(run => run.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<ProductionRun>> SearchStartedAsync(
        RunStatus? status,
        ulong? lineId,
        DateTime from,
        DateTime to,
        int take,
        CancellationToken cancellationToken) =>
        await Filtered(status, lineId)
            .Where(run => run.StartedAt != null && run.StartedAt <= to && (run.EndedAt == null || run.EndedAt >= from))
            .Include(run => run.Allocations)
            .Include(run => run.Outputs)
            .AsSplitQuery()
            .OrderByDescending(run => run.StartedAt)
            .ThenByDescending(run => run.Id)
            .Take(take)
            .ToListAsync(cancellationToken);

    private IQueryable<ProductionRun> Filtered(RunStatus? status, ulong? lineId) =>
        DbSet.AsNoTracking()
            .Where(run => status == null || run.Status == status)
            .Where(run => lineId == null || run.LineId == lineId);
}
