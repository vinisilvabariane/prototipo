using MeOne.Domain.Enums;
using MeOne.Domain.Errors;

namespace MeOne.Domain.Entities;

public sealed class ProductionRun : Entity<ulong>
{
    public const int MaxQuantity = 1_000_000;

    private readonly List<RunAllocation> _allocations = [];

    private readonly List<RunOutput> _outputs = [];

    private ProductionRun() { }

    public ulong LayoutId { get; private set; }

    public ulong LineId { get; private set; }

    public ulong ProductId { get; private set; }

    public ulong ShiftId { get; private set; }

    public RunStatus Status { get; private set; }

    public RunSnapshot Snapshot { get; private set; } = RunSnapshot.Empty;

    public DateTime CreatedAt { get; private set; }

    public ulong CreatedBy { get; private set; }

    public DateTime? StartedAt { get; private set; }

    public ulong? StartedBy { get; private set; }

    public DateTime? EndedAt { get; private set; }

    public ulong? EndedBy { get; private set; }

    public IReadOnlyList<RunAllocation> Allocations => _allocations;

    public IReadOnlyList<RunOutput> Outputs => _outputs;

    public IReadOnlyList<RunAllocation> OpenAllocations => [.. _allocations.Where(allocation => allocation.IsOpen)];

    public bool IsActive => Status != RunStatus.Closed;

    public static ProductionRun Create(LineLayout layout, ulong userId, DateTime now) => new()
    {
        LayoutId = layout.Id,
        LineId = layout.LineId,
        ProductId = layout.ProductId,
        ShiftId = layout.ShiftId,
        Status = RunStatus.Preparing,
        Snapshot = RunSnapshot.From(layout),
        CreatedAt = now,
        CreatedBy = userId,
    };

    public RunAllocation? OpenAllocationOf(ulong postId) =>
        _allocations.FirstOrDefault(allocation => allocation.IsOpen && allocation.PostId == postId);

    public string? CheckAllocate(ulong postId, ulong userId, int maxMachinePosts)
    {
        if (Status == RunStatus.Closed)
            return ProductionErrors.RunClosed;

        if (Snapshot.FindPost(postId) is not { } post)
            return ProductionErrors.PostNotInRun;

        if (OpenAllocationOf(postId)?.UserId == userId)
            return null;

        var others = _allocations.Where(allocation => allocation.IsOpen && allocation.UserId == userId && allocation.PostId != postId).ToList();

        if (post.Kind == PostKind.Machine)
        {
            if (others.Any(allocation => Snapshot.FindPost(allocation.PostId)?.Kind != PostKind.Machine))
                return ProductionErrors.PersonHoldsCommonPost;

            return others.Count >= maxMachinePosts ? ProductionErrors.MachineLimit(maxMachinePosts) : null;
        }

        return others.Count > 0 ? ProductionErrors.PersonAlreadyAllocated : null;
    }

    public bool Allocate(ulong postId, ulong userId, ulong allocatedBy, DateTime now)
    {
        var current = OpenAllocationOf(postId);

        if (current?.UserId == userId)
            return false;

        current?.Release(now);
        _allocations.Add(RunAllocation.Create(postId, userId, allocatedBy, now));
        return true;
    }

    public bool Release(ulong postId, DateTime now)
    {
        if (OpenAllocationOf(postId) is not { } current)
            return false;

        current.Release(now);
        return true;
    }

    public int ReleaseAll(DateTime now)
    {
        var open = OpenAllocations;

        foreach (var allocation in open)
            allocation.Release(now);

        return open.Count;
    }

    public (int Allocated, int Skipped) CopyAllocationsFrom(
        ProductionRun previous,
        Func<ulong, bool> userIsFree,
        int maxMachinePosts,
        ulong allocatedBy,
        DateTime now)
    {
        var allocated = 0;
        var skipped = 0;

        var roster = previous._allocations
            .Where(allocation => previous.EndedAt is { } ended ? allocation.ReleasedAt == ended : allocation.IsOpen)
            .ToList();

        foreach (var entry in roster)
        {
            var previousPost = previous.Snapshot.FindPost(entry.PostId);
            var post = previousPost is null
                ? null
                : Snapshot.Posts.FirstOrDefault(candidate => string.Equals(candidate.Name, previousPost.Name, StringComparison.CurrentCultureIgnoreCase));

            if (post is null || !userIsFree(entry.UserId) || CheckAllocate(post.Id, entry.UserId, maxMachinePosts) is not null)
            {
                skipped++;
                continue;
            }

            if (Allocate(post.Id, entry.UserId, allocatedBy, now))
                allocated++;
        }

        return (allocated, skipped);
    }

    public string? CheckStart()
    {
        if (Status != RunStatus.Preparing)
            return ProductionErrors.RunNotPreparing;

        var open = OpenAllocations;

        if (open.Count == 0)
            return ProductionErrors.RunWithoutAllocations;

        var missing = Snapshot.Posts
            .Where(post => post.IsCritical && open.All(allocation => allocation.PostId != post.Id))
            .Select(post => post.Name)
            .ToList();

        return missing.Count > 0 ? ProductionErrors.CriticalPostsEmpty(missing) : null;
    }

    public void Start(ulong userId, DateTime now)
    {
        Status = RunStatus.Running;
        StartedAt = now;
        StartedBy = userId;
    }

    public string? CheckClose() => Status == RunStatus.Running ? null : ProductionErrors.RunNotRunning;

    public void Close(ulong userId, DateTime now)
    {
        ReleaseAll(now);
        Status = RunStatus.Closed;
        EndedAt = now;
        EndedBy = userId;
    }

    public string? CheckCancel() => Status == RunStatus.Preparing ? null : ProductionErrors.RunCannotBeCancelled;

    public static DateTime TruncateToHour(DateTime value) =>
        new(value.Year, value.Month, value.Day, value.Hour, 0, 0, DateTimeKind.Utc);

    public string? CheckRecordOutput(ulong postId, int quantity, DateTime hourStart, DateTime now)
    {
        if (Status != RunStatus.Running)
            return ProductionErrors.RunNotRunning;

        if (Snapshot.FindPost(postId) is null)
            return ProductionErrors.PostNotInRun;

        if (quantity is < 0 or > MaxQuantity)
            return ProductionErrors.QuantityInvalid;

        var slot = TruncateToHour(hourStart);

        if (slot > TruncateToHour(now))
            return ProductionErrors.OutputInTheFuture;

        return slot < TruncateToHour(StartedAt!.Value) ? ProductionErrors.OutputBeforeStart : null;
    }

    public RunOutput RecordOutput(ulong postId, int quantity, DateTime hourStart, ulong recordedBy, DateTime now)
    {
        var slot = TruncateToHour(hourStart);
        var holder = HolderDuring(postId, slot);

        var existing = _outputs.FirstOrDefault(output => output.PostId == postId && output.HourStart == slot);

        if (existing is not null)
        {
            existing.Set(quantity, holder, recordedBy, now);
            return existing;
        }

        var created = RunOutput.Create(postId, slot, quantity, holder, recordedBy, now);
        _outputs.Add(created);
        return created;
    }

    private ulong? HolderDuring(ulong postId, DateTime slot)
    {
        var slotEnd = slot.AddHours(1);

        return _allocations
            .Where(allocation => allocation.PostId == postId)
            .Select(allocation => (allocation.UserId, Overlap: Overlap(allocation.AllocatedAt, allocation.ReleasedAt ?? slotEnd, slot, slotEnd)))
            .Where(candidate => candidate.Overlap > TimeSpan.Zero)
            .OrderByDescending(candidate => candidate.Overlap)
            .Select(candidate => (ulong?)candidate.UserId)
            .FirstOrDefault();
    }

    private static TimeSpan Overlap(DateTime from, DateTime to, DateTime windowFrom, DateTime windowTo)
    {
        var start = from > windowFrom ? from : windowFrom;
        var end = to < windowTo ? to : windowTo;

        return end > start ? end - start : TimeSpan.Zero;
    }
}
