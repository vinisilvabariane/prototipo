using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Oee.Responses;

public sealed record RunOeeResponse(
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
    OeePillarsResponse Line,
    double Produced,
    double Theoretical,
    ulong? LastPostId,
    IReadOnlyList<OeePostResponse> Posts,
    IReadOnlyList<OeeHourResponse> Hours,
    bool LossSourceAvailable,
    string FormulaSetName);
