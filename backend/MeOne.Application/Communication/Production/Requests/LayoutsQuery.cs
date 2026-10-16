namespace MeOne.Application.Communication.Production.Requests;

public sealed record LayoutsQuery
{
    public ulong? LineId { get; init; }

    public ulong? ProductId { get; init; }

    public ulong? ShiftId { get; init; }
}
