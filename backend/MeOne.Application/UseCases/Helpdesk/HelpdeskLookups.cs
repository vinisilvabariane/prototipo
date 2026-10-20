using FluentResults;
using MeOne.Application.Errors;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Helpdesk;

internal static class HelpdeskLookups
{
    public static async Task<Result<HelpdeskSector>> SectorAsync(IHelpdeskSectorRepository sectors, ulong id, CancellationToken cancellationToken)
    {
        var sector = await sectors.GetByIdAsync(id, cancellationToken);

        return sector is null
            ? Result.Fail<HelpdeskSector>(new NotFoundError(HelpdeskErrors.SectorNotFound))
            : Result.Ok(sector);
    }

    public static async Task<Result<HelpdeskTicket>> TicketAsync(IHelpdeskTicketRepository tickets, ulong id, CancellationToken cancellationToken)
    {
        var ticket = await tickets.GetByIdAsync(id, cancellationToken);

        return ticket is null
            ? Result.Fail<HelpdeskTicket>(new NotFoundError(HelpdeskErrors.TicketNotFound))
            : Result.Ok(ticket);
    }

    public static async Task<Dictionary<ulong, string>> LineNamesAsync(
        IProductionLineRepository lines,
        IEnumerable<ulong> ids,
        CancellationToken cancellationToken)
    {
        var distinct = ids.Distinct().ToList();

        return distinct.Count == 0
            ? []
            : (await lines.GetByIdsAsync(distinct, cancellationToken)).ToDictionary(line => line.Id, line => line.Name);
    }

    public static async Task<HelpdeskTicketContext> ContextAsync(
        IEnumerable<HelpdeskTicket> tickets,
        IHelpdeskSectorRepository sectors,
        IUserRepository users,
        IProductionLineRepository lines,
        CancellationToken cancellationToken)
    {
        var list = tickets.ToList();
        var sectorMap = (await sectors.GetByIdsAsync([.. list.Select(ticket => ticket.SectorId).Distinct()], cancellationToken)).ToDictionary(sector => sector.Id);
        var userIds = list.SelectMany(HelpdeskTicketMapping.UserIds).Distinct().ToList();
        var userMap = userIds.Count == 0
            ? []
            : (await users.GetByIdsIncludingInactiveAsync(userIds, cancellationToken)).ToDictionary(user => user.Id);
        var lineMap = await LineNamesAsync(lines, list.Where(ticket => ticket.LineId is not null).Select(ticket => ticket.LineId!.Value), cancellationToken);

        return new(sectorMap, userMap, lineMap);
    }
}
