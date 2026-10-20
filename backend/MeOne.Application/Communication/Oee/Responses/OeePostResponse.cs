using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Oee.Responses;

public sealed record OeePostResponse(
    ulong PostId,
    string Name,
    PostKind Kind,
    int X,
    decimal? TargetPerHour,
    bool Measured,
    OeePillarsResponse Pillars,
    double Produced,
    double Theoretical,
    double OccupiedSeconds,
    double DowntimeSeconds,
    double Defects,
    IReadOnlyList<OeeHourResponse> Hours);
