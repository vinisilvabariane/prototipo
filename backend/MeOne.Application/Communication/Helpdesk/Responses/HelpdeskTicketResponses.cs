using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Helpdesk.Responses;

public sealed record HelpdeskAnswerView(string Label, HelpdeskFieldType Type, string Display, DateTime? At);

public sealed record HelpdeskTicketSummaryResponse(
    ulong Id,
    ulong SectorId,
    string SectorName,
    string SectorColor,
    string SectorIcon,
    HelpdeskTicketStatus Status,
    string OpenedByName,
    DateTime OpenedAt,
    ulong? LineId,
    string? LineName,
    string? PostName,
    bool IsLineStopped,
    DateTime? StoppedAt,
    string? AssignedToName,
    DateTime? StartedAt,
    DateTime? FinishedAt,
    IReadOnlyList<HelpdeskAnswerView> Summary,
    bool CanStart,
    bool CanResolve,
    bool CanReject);

public sealed record HelpdeskStatusCounts(int Open, int InProgress, int Resolved, int Rejected);

public sealed record HelpdeskTicketPageResponse(
    IReadOnlyList<HelpdeskTicketSummaryResponse> Items,
    int TotalCount,
    int Page,
    int PageSize,
    HelpdeskStatusCounts Counts,
    bool CanAttend);

public sealed record HelpdeskTicketDetailResponse(
    ulong Id,
    ulong SectorId,
    string SectorName,
    string SectorColor,
    string SectorIcon,
    HelpdeskTicketStatus Status,
    string OpenedByName,
    DateTime OpenedAt,
    ulong? LineId,
    string? LineName,
    string? PostName,
    bool IsLineStopped,
    DateTime? StoppedAt,
    int Defects,
    string? AssignedToName,
    DateTime? StartedAt,
    DateTime? FinishedAt,
    int? DowntimeSeconds,
    IReadOnlyList<HelpdeskAnswerView> Answers,
    IReadOnlyList<HelpdeskAnswerView> ResolutionAnswers,
    string? Note,
    string? RejectedByName,
    string? RejectReason,
    bool CanStart,
    bool CanResolve,
    bool CanReject,
    bool HasResolutionForm,
    HelpdeskFormDto ResolutionForm);
