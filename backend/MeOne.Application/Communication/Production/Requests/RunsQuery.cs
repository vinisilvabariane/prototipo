using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Production.Requests;

public sealed record RunsQuery
{
    public int Page { get; init; }

    public int PageSize { get; init; } = 10;

    public RunStatus? Status { get; init; }

    public ulong? LineId { get; init; }
}
