using MeOne.Domain.Enums;
using MeOne.Domain.Errors;
using MeOne.Domain.Rules;

namespace MeOne.Domain.Entities;

public sealed class HelpdeskTicket : Entity<ulong>
{
    private HelpdeskTicket() { }

    public ulong SectorId { get; private set; }

    public HelpdeskTicketStatus Status { get; private set; }

    public ulong OpenedBy { get; private set; }

    public DateTime OpenedAt { get; private set; }

    public ulong? LineId { get; private set; }

    public ulong? PostId { get; private set; }

    public string? PostName { get; private set; }

    public bool IsLineStopped { get; private set; }

    public DateTime? StoppedAt { get; private set; }

    public int Defects { get; private set; }

    public HelpdeskForm FormSnapshot { get; private set; } = HelpdeskForm.Empty;

    public IReadOnlyDictionary<string, string> Answers { get; private set; } = new Dictionary<string, string>();

    public ulong? AssignedTo { get; private set; }

    public DateTime? StartedAt { get; private set; }

    public HelpdeskForm ResolutionSnapshot { get; private set; } = HelpdeskForm.Empty;

    public IReadOnlyDictionary<string, string> ResolutionAnswers { get; private set; } = new Dictionary<string, string>();

    public string? Note { get; private set; }

    public DateTime? FinishedAt { get; private set; }

    public ulong? RejectedBy { get; private set; }

    public string? RejectReason { get; private set; }

    public bool IsClosed => Status is HelpdeskTicketStatus.Resolved or HelpdeskTicketStatus.Rejected;

    public DateTime? DowntimeStart => Status == HelpdeskTicketStatus.Rejected || !IsLineStopped ? null : StoppedAt;

    public DateTime DowntimeEnd(DateTime now) => FinishedAt ?? now;

    public static HelpdeskTicket Open(
        HelpdeskSector sector,
        ulong userId,
        IReadOnlyDictionary<string, string> answers,
        HelpdeskSpot spot,
        string? postName,
        DateTime now) => new()
    {
        SectorId = sector.Id,
        Status = HelpdeskTicketStatus.Open,
        OpenedBy = userId,
        OpenedAt = now,
        LineId = spot.LineId,
        PostId = spot.PostId,
        PostName = postName,
        IsLineStopped = spot.Stopped,
        StoppedAt = spot.Stopped ? spot.StoppedAt ?? now : null,
        Defects = spot.Defects,
        FormSnapshot = sector.Form,
        Answers = answers,
    };

    public string? CheckStart() => Status == HelpdeskTicketStatus.Open ? null : HelpdeskErrors.TicketNotOpen;

    public void Start(ulong userId, DateTime now)
    {
        Status = HelpdeskTicketStatus.InProgress;
        AssignedTo = userId;
        StartedAt = now;
    }

    public string? CheckResolve(ulong userId)
    {
        if (Status != HelpdeskTicketStatus.InProgress)
            return HelpdeskErrors.TicketNotInProgress;

        return AssignedTo == userId ? null : HelpdeskErrors.OnlyAssigneeResolves;
    }

    public void Resolve(HelpdeskForm resolutionForm, IReadOnlyDictionary<string, string> resolutionAnswers, string? note, DateTime now)
    {
        Status = HelpdeskTicketStatus.Resolved;
        ResolutionSnapshot = resolutionForm;
        ResolutionAnswers = resolutionAnswers;
        Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        FinishedAt = now;
    }

    public string? CheckReject() => Status == HelpdeskTicketStatus.Open ? null : HelpdeskErrors.TicketNotOpen;

    public void Reject(ulong userId, string reason, DateTime now)
    {
        Status = HelpdeskTicketStatus.Rejected;
        RejectedBy = userId;
        RejectReason = reason.Trim();
        FinishedAt = now;
    }
}
