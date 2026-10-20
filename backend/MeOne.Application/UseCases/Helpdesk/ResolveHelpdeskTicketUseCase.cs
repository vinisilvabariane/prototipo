using FluentResults;
using MeOne.Application.Communication.Helpdesk.Requests;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Helpdesk;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Helpdesk;

public sealed class ResolveHelpdeskTicketUseCase(
    IHelpdeskTicketRepository tickets,
    IHelpdeskSectorRepository sectors,
    IUserRepository users,
    ICurrentActor currentActor) : IResolveHelpdeskTicketUseCase
{
    public async Task<Result> ExecuteAsync(ulong id, ResolveHelpdeskTicketRequest request, CancellationToken cancellationToken)
    {
        var actor = await HelpdeskActor.ResolveAsync(currentActor, users, cancellationToken);
        if (actor.IsFailed)
            return actor.ToResult();

        var ticket = await HelpdeskLookups.TicketAsync(tickets, id, cancellationToken);
        if (ticket.IsFailed)
            return ticket.ToResult();

        if (ticket.Value.CheckResolve(actor.Value.UserId) is { } problem)
            return Result.Fail(problem);

        var sector = await HelpdeskLookups.SectorAsync(sectors, ticket.Value.SectorId, cancellationToken);
        if (sector.IsFailed)
            return sector.ToResult();

        var form = sector.Value.ResolutionForm;
        var now = DateTime.UtcNow;
        IReadOnlyDictionary<string, string> answers = new Dictionary<string, string>();

        if (!form.IsEmpty)
        {
            var normalized = HelpdeskAnswers.Normalize(form, request.Answers ?? new Dictionary<string, string?>(), now);

            if (normalized.Failed)
                return Result.Fail(normalized.Problem!);

            answers = normalized.Answers;
        }

        ticket.Value.Resolve(form, answers, request.Note, now);
        tickets.Update(ticket.Value);

        return await tickets.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok()
            : Result.Fail(HelpdeskErrors.NothingPersisted);
    }
}
