using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Oee.Responses;

public sealed record OeeRunSummaryResponse(
    ulong RunId,
    RunStatus Status,
    ulong LineId,
    string LineName,
    string ProductCode,
    string ProductName,
    string ShiftName,
    DateTime? StartedAt,
    DateTime? EndedAt,
    int ElapsedSeconds,
    OeePillarsResponse Pillars,
    double Produced,
    double Theoretical,
    int MeasuredPosts,
    int TotalPosts);
