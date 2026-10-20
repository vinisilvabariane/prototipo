using FluentResults;
using MeOne.Application.Communication.Helpdesk.Requests;
using MeOne.Application.Communication.Helpdesk.Responses;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Helpdesk;
using MeOne.Domain.Entities;
using MeOne.Domain.Enums;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Helpdesk;

public sealed class OpenHelpdeskTicketUseCase(
    IHelpdeskTicketRepository tickets,
    IHelpdeskSectorRepository sectors,
    IProductionLineRepository lines,
    IProductionRunRepository runs,
    IUserRepository users,
    ICurrentActor currentActor) : IOpenHelpdeskTicketUseCase
{
    public async Task<Result<HelpdeskTicketSummaryResponse>> ExecuteAsync(OpenHelpdeskTicketRequest request, CancellationToken cancellationToken)
    {
        var actor = await HelpdeskActor.ResolveAsync(currentActor, users, cancellationToken);
        if (actor.IsFailed)
            return actor.ToResult<HelpdeskTicketSummaryResponse>();

        var sector = await HelpdeskLookups.SectorAsync(sectors, request.SectorId, cancellationToken);
        if (sector.IsFailed)
            return sector.ToResult<HelpdeskTicketSummaryResponse>();

        if (!sector.Value.Active)
            return Result.Fail<HelpdeskTicketSummaryResponse>(HelpdeskErrors.SectorInactive);

        if (sector.Value.IsExternal)
            return Result.Fail<HelpdeskTicketSummaryResponse>(HelpdeskErrors.SectorIsExternal);

        if (sector.Value.Form.IsEmpty)
            return Result.Fail<HelpdeskTicketSummaryResponse>(HelpdeskErrors.SectorHasNoForm);

        var now = DateTime.UtcNow;
        var normalized = HelpdeskAnswers.Normalize(sector.Value.Form, request.Answers ?? new Dictionary<string, string?>(), now);

        if (normalized.Failed)
            return Result.Fail<HelpdeskTicketSummaryResponse>(normalized.Problem!);

        var spot = HelpdeskAnswers.ReadSpot(sector.Value.Form, normalized.Answers);
        string? postName = null;

        if (spot.LineId is { } lineId)
        {
            var line = await lines.GetByIdAsync(lineId, cancellationToken);
            if (line is null || !line.Active)
                return Result.Fail<HelpdeskTicketSummaryResponse>(HelpdeskErrors.LineNotFound);

            if (spot.PostId is { } postId)
            {
                var running = await runs.SearchAsync(RunStatus.Running, lineId, 0, 1, cancellationToken);
                postName = running.FirstOrDefault()?.Snapshot.FindPost(postId)?.Name;

                if (postName is null)
                    return Result.Fail<HelpdeskTicketSummaryResponse>(HelpdeskErrors.PostNotInLine);
            }
        }
        else if (spot.Stopped)
        {
            return Result.Fail<HelpdeskTicketSummaryResponse>(HelpdeskErrors.LineRequired);
        }

        var ticket = HelpdeskTicket.Open(sector.Value, actor.Value.UserId, normalized.Answers, spot, postName, now);
        await tickets.AddAsync(ticket, cancellationToken);

        if (await tickets.SaveChangesAsync(cancellationToken) <= 0)
            return Result.Fail<HelpdeskTicketSummaryResponse>(HelpdeskErrors.NothingPersisted);

        var context = await HelpdeskLookups.ContextAsync([ticket], sectors, users, lines, cancellationToken);

        return Result.Ok(HelpdeskTicketMapping.ToSummary(ticket, context, actor.Value));
    }
}
