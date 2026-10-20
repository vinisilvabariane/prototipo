using FluentResults;
using MeOne.Application.Communication.Helpdesk.Responses;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Helpdesk;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Helpdesk;

public sealed class GetHelpdeskTicketUseCase(
    IHelpdeskTicketRepository tickets,
    IHelpdeskSectorRepository sectors,
    IProductionLineRepository lines,
    IUserRepository users,
    ICurrentActor currentActor) : IGetHelpdeskTicketUseCase
{
    public async Task<Result<HelpdeskTicketDetailResponse>> ExecuteAsync(ulong id, CancellationToken cancellationToken)
    {
        var actor = await HelpdeskActor.ResolveAsync(currentActor, users, cancellationToken);
        if (actor.IsFailed)
            return actor.ToResult<HelpdeskTicketDetailResponse>();

        var ticket = await HelpdeskLookups.TicketAsync(tickets, id, cancellationToken);
        if (ticket.IsFailed)
            return ticket.ToResult<HelpdeskTicketDetailResponse>();

        var context = await HelpdeskLookups.ContextAsync([ticket.Value], sectors, users, lines, cancellationToken);
        if (!context.Sectors.TryGetValue(ticket.Value.SectorId, out var sector))
            return Result.Fail<HelpdeskTicketDetailResponse>(new NotFoundError(HelpdeskErrors.SectorNotFound));

        if (ticket.Value.OpenedBy != actor.Value.UserId && !actor.Value.CanAttend(sector))
            return Result.Fail<HelpdeskTicketDetailResponse>(new ForbiddenError(HelpdeskErrors.CannotAttend));

        return Result.Ok(HelpdeskTicketMapping.ToDetail(ticket.Value, context, actor.Value, DateTime.UtcNow));
    }
}
