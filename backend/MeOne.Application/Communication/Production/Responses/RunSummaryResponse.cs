using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Production.Responses;

public sealed record RunSummaryResponse(
    ulong Id,
    RunStatus Status,
    ulong LineId,
    string LineName,
    string ProductCode,
    string ProductName,
    string ShiftName,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? EndedAt,
    int AllocatedPosts,
    int TotalPosts,
    int Produced);
