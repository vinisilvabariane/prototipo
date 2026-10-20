using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Helpdesk.Requests;

public sealed record HelpdeskTicketsQuery
{
    public int Page { get; init; }

    public int PageSize { get; init; } = 20;

    public HelpdeskScope Scope { get; init; } = HelpdeskScope.Attend;

    public HelpdeskTicketStatus? Status { get; init; }

    public ulong? SectorId { get; init; }

    public ulong? LineId { get; init; }

    public DateTime? From { get; init; }

    public DateTime? To { get; init; }
}
