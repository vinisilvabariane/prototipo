using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Production.Responses;

public sealed record RunDetailResponse(
    ulong Id,
    RunStatus Status,
    ulong LayoutId,
    ulong LineId,
    string LineName,
    string ProductCode,
    string ProductName,
    string ShiftName,
    DateTime CreatedAt,
    string CreatedBy,
    DateTime? StartedAt,
    string? StartedBy,
    DateTime? EndedAt,
    string? EndedBy,
    IReadOnlyList<RunPostResponse> Posts,
    IReadOnlyList<RunDecorationResponse> Decorations,
    IReadOnlyList<RunPersonResponse> People,
    IReadOnlyList<RunHourResponse> Hours,
    int TotalProduced,
    bool CanSupervise,
    bool CanRecord);
