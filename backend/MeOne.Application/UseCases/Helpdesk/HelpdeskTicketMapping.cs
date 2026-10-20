using MeOne.Application.Communication.Helpdesk.Responses;
using MeOne.Domain.Entities;
using MeOne.Domain.Enums;

namespace MeOne.Application.UseCases.Helpdesk;

internal sealed record HelpdeskTicketContext(
    IReadOnlyDictionary<ulong, HelpdeskSector> Sectors,
    IReadOnlyDictionary<ulong, User> Users,
    IReadOnlyDictionary<ulong, string> LineNames);

internal static class HelpdeskTicketMapping
{
    public static bool CanStart(HelpdeskTicket ticket, HelpdeskSector sector, HelpdeskActor actor) =>
        ticket.Status == HelpdeskTicketStatus.Open && actor.CanAttend(sector);

    public static bool CanReject(HelpdeskTicket ticket, HelpdeskSector sector, HelpdeskActor actor) =>
        ticket.Status == HelpdeskTicketStatus.Open && actor.CanAttend(sector);

    public static bool CanResolve(HelpdeskTicket ticket, HelpdeskActor actor) =>
        ticket.Status == HelpdeskTicketStatus.InProgress && ticket.AssignedTo == actor.UserId;

    private static string NameOf(IReadOnlyDictionary<ulong, User> users, ulong? id) =>
        id is { } value && users.TryGetValue(value, out var user) ? user.Name : string.Empty;

    private static string? OptionalName(IReadOnlyDictionary<ulong, User> users, ulong? id) =>
        id is null ? null : NameOf(users, id);

    public static IEnumerable<ulong> UserIds(HelpdeskTicket ticket) =>
        new ulong?[] { ticket.OpenedBy, ticket.AssignedTo, ticket.RejectedBy }.Where(id => id is not null).Select(id => id!.Value);

    public static HelpdeskTicketSummaryResponse ToSummary(HelpdeskTicket ticket, HelpdeskTicketContext context, HelpdeskActor actor)
    {
        var sector = context.Sectors[ticket.SectorId];

        return new(
            ticket.Id,
            sector.Id,
            sector.Name,
            sector.ColorHex,
            sector.Icon,
            ticket.Status,
            NameOf(context.Users, ticket.OpenedBy),
            ticket.OpenedAt,
            ticket.LineId,
            ticket.LineId is { } lineId && context.LineNames.TryGetValue(lineId, out var lineName) ? lineName : null,
            ticket.PostName,
            ticket.IsLineStopped,
            ticket.StoppedAt,
            OptionalName(context.Users, ticket.AssignedTo),
            ticket.StartedAt,
            ticket.FinishedAt,
            HelpdeskMapping.Views(ticket.FormSnapshot, ticket.Answers, context.LineNames, ticket.PostName, sector.SummaryFields),
            CanStart(ticket, sector, actor),
            CanResolve(ticket, actor),
            CanReject(ticket, sector, actor));
    }

    public static HelpdeskTicketDetailResponse ToDetail(HelpdeskTicket ticket, HelpdeskTicketContext context, HelpdeskActor actor, DateTime now)
    {
        var sector = context.Sectors[ticket.SectorId];
        var downtime = ticket.DowntimeStart is { } start ? (int?)Math.Max(0, (ticket.DowntimeEnd(now) - start).TotalSeconds) : null;
        var resolutionForm = ticket.Status == HelpdeskTicketStatus.Resolved ? ticket.ResolutionSnapshot : sector.ResolutionForm;

        return new(
            ticket.Id,
            sector.Id,
            sector.Name,
            sector.ColorHex,
            sector.Icon,
            ticket.Status,
            NameOf(context.Users, ticket.OpenedBy),
            ticket.OpenedAt,
            ticket.LineId,
            ticket.LineId is { } lineId && context.LineNames.TryGetValue(lineId, out var lineName) ? lineName : null,
            ticket.PostName,
            ticket.IsLineStopped,
            ticket.StoppedAt,
            ticket.Defects,
            OptionalName(context.Users, ticket.AssignedTo),
            ticket.StartedAt,
            ticket.FinishedAt,
            downtime,
            HelpdeskMapping.Views(ticket.FormSnapshot, ticket.Answers, context.LineNames, ticket.PostName),
            HelpdeskMapping.Views(ticket.ResolutionSnapshot, ticket.ResolutionAnswers, context.LineNames, null),
            ticket.Note,
            OptionalName(context.Users, ticket.RejectedBy),
            ticket.RejectReason,
            CanStart(ticket, sector, actor),
            CanResolve(ticket, actor),
            CanReject(ticket, sector, actor),
            !resolutionForm.IsEmpty,
            HelpdeskMapping.ToDto(resolutionForm));
    }
}
