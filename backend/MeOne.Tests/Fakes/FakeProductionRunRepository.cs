using MeOne.Domain.Entities;
using MeOne.Domain.Enums;
using MeOne.Domain.Interfaces;

namespace MeOne.Tests.Fakes;

public sealed class FakeProductionRunRepository : InMemoryRepository<ProductionRun, ulong>, IProductionRunRepository
{
    private ulong _lastChildIdentity;

    public Task<bool> LineHasActiveRunAsync(ulong lineId, CancellationToken cancellationToken) =>
        Task.FromResult(Items.Any(run => run.LineId == lineId && run.IsActive));

    public Task<IReadOnlyDictionary<ulong, ulong>> FindActiveRunsOfUsersAsync(
        IReadOnlyCollection<ulong> userIds,
        ulong exceptRunId,
        CancellationToken cancellationToken)
    {
        var found = Items
            .Where(run => run.IsActive && run.Id != exceptRunId)
            .SelectMany(run => run.OpenAllocations.Select(allocation => (allocation.UserId, run.LineId)))
            .Where(entry => userIds.Contains(entry.UserId))
            .GroupBy(entry => entry.UserId)
            .ToDictionary(group => group.Key, group => group.First().LineId);

        return Task.FromResult<IReadOnlyDictionary<ulong, ulong>>(found);
    }

    public Task<ProductionRun?> GetLastClosedOfLayoutAsync(ulong layoutId, CancellationToken cancellationToken) =>
        Task.FromResult(Items
            .Where(run => run.LayoutId == layoutId && run.Status == RunStatus.Closed)
            .OrderByDescending(run => run.EndedAt)
            .ThenByDescending(run => run.Id)
            .FirstOrDefault());

    public Task<int> CountAsync(RunStatus? status, ulong? lineId, CancellationToken cancellationToken) =>
        Task.FromResult(Filtered(status, lineId).Count());

    public Task<IReadOnlyList<ProductionRun>> SearchAsync(RunStatus? status, ulong? lineId, int skip, int take, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ProductionRun>>(
            [.. Filtered(status, lineId).OrderByDescending(run => run.CreatedAt).ThenByDescending(run => run.Id).Skip(skip).Take(take)]);

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        foreach (var run in Items)
        {
            foreach (var allocation in run.Allocations.Where(item => item.Id == 0))
                allocation.Id = ++_lastChildIdentity;

            foreach (var output in run.Outputs.Where(item => item.Id == 0))
                output.Id = ++_lastChildIdentity;
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    private IEnumerable<ProductionRun> Filtered(RunStatus? status, ulong? lineId) =>
        Items.Where(run => status is null || run.Status == status).Where(run => lineId is null || run.LineId == lineId);
}
