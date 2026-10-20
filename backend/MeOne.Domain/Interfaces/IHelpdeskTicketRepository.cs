using MeOne.Domain.Entities;
using MeOne.Domain.Enums;

namespace MeOne.Domain.Interfaces;

public sealed record HelpdeskTicketFilter(
    IReadOnlyCollection<ulong>? SectorIds,
    ulong? OpenedBy,
    HelpdeskTicketStatus? Status,
    ulong? SectorId,
    ulong? LineId,
    DateTime? From,
    DateTime? To);

public interface IHelpdeskTicketRepository : IRepository<HelpdeskTicket, ulong>
{
    Task<int> CountAsync(HelpdeskTicketFilter filter, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<HelpdeskTicketStatus, int>> CountByStatusAsync(HelpdeskTicketFilter filter, CancellationToken cancellationToken);

    Task<IReadOnlyList<HelpdeskTicket>> SearchAsync(HelpdeskTicketFilter filter, int skip, int take, CancellationToken cancellationToken);

    Task<IReadOnlyList<HelpdeskTicket>> ListLineLossesAsync(ulong lineId, DateTime from, DateTime to, CancellationToken cancellationToken);

    Task<bool> SectorHasTicketsAsync(ulong sectorId, CancellationToken cancellationToken);
}
