namespace MeOne.Domain.Rules;

public enum OeeDowntimeSource
{
    Ticket,
    Real,
}

public sealed record OeeDowntimeEvent(
    ulong? PostId,
    DateTime From,
    DateTime To,
    bool AffectsAvailability,
    bool AffectsPerformance,
    OeeDowntimeSource Source);

public sealed record OeeDefectEvent(ulong? PostId, DateTime At, int Quantity, bool IsRepair);

public sealed record OeeLosses(IReadOnlyList<OeeDowntimeEvent> Downtimes, IReadOnlyList<OeeDefectEvent> Defects, bool Available)
{
    public static OeeLosses None { get; } = new([], [], false);
}
