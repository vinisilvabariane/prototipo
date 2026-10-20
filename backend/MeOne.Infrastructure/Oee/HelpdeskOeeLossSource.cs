using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Infrastructure.Oee;

public sealed class HelpdeskOeeLossSource(IHelpdeskTicketRepository tickets) : IOeeLossSource
{
    public async Task<OeeLosses> GetAsync(ulong lineId, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        if (lineId == 0)
            return new([], [], true);

        var found = await tickets.ListLineLossesAsync(lineId, from, to, cancellationToken);

        var downtimes = found
            .Where(ticket => ticket.DowntimeStart is not null)
            .Select(ticket => new OeeDowntimeEvent(ticket.PostId, ticket.DowntimeStart!.Value, ticket.DowntimeEnd(to), true, false, OeeDowntimeSource.Ticket))
            .ToList();

        var defects = found
            .Where(ticket => ticket.Defects > 0)
            .Select(ticket => new OeeDefectEvent(ticket.PostId, ticket.OpenedAt, ticket.Defects, false))
            .ToList();

        return new(downtimes, defects, true);
    }
}
