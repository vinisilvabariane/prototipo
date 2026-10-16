namespace MeOne.Domain.Entities;

public sealed class RunOutput : Entity<ulong>
{
    private RunOutput() { }

    public ulong RunId { get; private set; }

    public ulong PostId { get; private set; }

    public DateTime HourStart { get; private set; }

    public int Quantity { get; private set; }

    public ulong? UserId { get; private set; }

    public ulong RecordedBy { get; private set; }

    public DateTime RecordedAt { get; private set; }

    internal static RunOutput Create(ulong postId, DateTime hourStart, int quantity, ulong? userId, ulong recordedBy, DateTime now) => new()
    {
        PostId = postId,
        HourStart = hourStart,
        Quantity = quantity,
        UserId = userId,
        RecordedBy = recordedBy,
        RecordedAt = now,
    };

    internal void Set(int quantity, ulong? userId, ulong recordedBy, DateTime now)
    {
        Quantity = quantity;
        UserId = userId;
        RecordedBy = recordedBy;
        RecordedAt = now;
    }
}
