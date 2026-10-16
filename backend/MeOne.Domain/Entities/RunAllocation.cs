namespace MeOne.Domain.Entities;

public sealed class RunAllocation : Entity<ulong>
{
    private RunAllocation() { }

    public ulong RunId { get; private set; }

    public ulong PostId { get; private set; }

    public ulong UserId { get; private set; }

    public DateTime AllocatedAt { get; private set; }

    public DateTime? ReleasedAt { get; private set; }

    public ulong AllocatedBy { get; private set; }

    public bool IsOpen => ReleasedAt is null;

    internal static RunAllocation Create(ulong postId, ulong userId, ulong allocatedBy, DateTime now) => new()
    {
        PostId = postId,
        UserId = userId,
        AllocatedBy = allocatedBy,
        AllocatedAt = now,
    };

    internal void Release(DateTime now) => ReleasedAt = now;
}
