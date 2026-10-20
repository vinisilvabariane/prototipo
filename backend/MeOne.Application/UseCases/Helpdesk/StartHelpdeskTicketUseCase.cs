using FluentResults;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Helpdesk;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Helpdesk;

public sealed class StartHelpdeskTicketUseCase(
    IHelpdeskTicketRepository tickets,
    IHelpdeskSectorRepository sectors,
    IUserRepository users,
    ICurrentActor currentActor) : IStartHelpdeskTicketUseCase
{
    public async Task<Result> ExecuteAsync(ulong id, CancellationToken cancellationToken)
    {
        var actor = await HelpdeskActor.ResolveAsync(currentActor, users, cancellationToken);
        if (actor.IsFailed)
            return actor.ToResult();

        var ticket = await HelpdeskLookups.TicketAsync(tickets, id, cancellationToken);
        if (ticket.IsFailed)
            return ticket.ToResult();

        var sector = await HelpdeskLookups.SectorAsync(sectors, ticket.Value.SectorId, cancellationToken);
        if (sector.IsFailed)
            return sector.ToResult();

        if (!actor.Value.CanAttend(sector.Value))
            return Result.Fail(new ForbiddenError(HelpdeskErrors.CannotAttend));

        if (ticket.Value.CheckStart() is { } problem)
            return Result.Fail(problem);

        ticket.Value.Start(actor.Value.UserId, DateTime.UtcNow);
        tickets.Update(ticket.Value);

        return await tickets.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok()
            : Result.Fail(HelpdeskErrors.NothingPersisted);
    }
}
