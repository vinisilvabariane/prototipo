using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Oee.Requests;

public sealed record OeeRunsQuery
{
    public DateTime? From { get; init; }

    public DateTime? To { get; init; }

    public RunStatus? Status { get; init; }

    public ulong? LineId { get; init; }
}
