using FluentResults;
using MeOne.Application.Communication.Helpdesk.Requests;
using MeOne.Application.Communication.Helpdesk.Responses;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Helpdesk;
using MeOne.Domain.Entities;
using MeOne.Domain.Enums;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Helpdesk;

public sealed class ListHelpdeskTicketsUseCase(
    IHelpdeskTicketRepository tickets,
    IHelpdeskSectorRepository sectors,
    IProductionLineRepository lines,
    IUserRepository users,
    ICurrentActor currentActor) : IListHelpdeskTicketsUseCase
{
    public async Task<Result<HelpdeskTicketPageResponse>> ExecuteAsync(HelpdeskTicketsQuery query, CancellationToken cancellationToken)
    {
        var actor = await HelpdeskActor.ResolveAsync(currentActor, users, cancellationToken);
        if (actor.IsFailed)
            return actor.ToResult<HelpdeskTicketPageResponse>();

        if (query.From is { } from && query.To is { } to && to < from)
            return Result.Fail<HelpdeskTicketPageResponse>(HelpdeskErrors.PeriodInvalid);

        var all = await sectors.ListAsync(true, cancellationToken);
        var attended = all.Where(sector => actor.Value.CanAttend(sector)).Select(sector => sector.Id).ToList();
        var canAttend = attended.Count > 0;

        var mine = query.Scope == HelpdeskScope.Mine;
        var empty = !mine && !canAttend;

        var filter = new HelpdeskTicketFilter(
            mine || actor.Value.IsAdministrator ? null : attended,
            mine ? actor.Value.UserId : null,
            query.Status,
            query.SectorId,
            query.LineId,
            query.From is { } start ? DateTime.SpecifyKind(start.Date, DateTimeKind.Utc) : null,
            query.To is { } end ? DateTime.SpecifyKind(end.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc) : null);

        if (empty)
            return Result.Ok(new HelpdeskTicketPageResponse([], 0, query.Page, query.PageSize, new(0, 0, 0, 0), false));

        var total = await tickets.CountAsync(filter, cancellationToken);
        var found = await tickets.SearchAsync(filter, query.Page * query.PageSize, query.PageSize, cancellationToken);
        var counts = await tickets.CountByStatusAsync(filter with { Status = null }, cancellationToken);
        var context = await HelpdeskLookups.ContextAsync(found, sectors, users, lines, cancellationToken);

        var items = found
            .Where(ticket => context.Sectors.ContainsKey(ticket.SectorId))
            .Select(ticket => HelpdeskTicketMapping.ToSummary(ticket, context, actor.Value))
            .ToList();

        int Count(HelpdeskTicketStatus status) => counts.GetValueOrDefault(status);

        return Result.Ok(new HelpdeskTicketPageResponse(
            items,
            total,
            query.Page,
            query.PageSize,
            new(Count(HelpdeskTicketStatus.Open), Count(HelpdeskTicketStatus.InProgress), Count(HelpdeskTicketStatus.Resolved), Count(HelpdeskTicketStatus.Rejected)),
            canAttend));
    }
}
