using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Production.Responses;

public sealed record RunPostResponse(
    ulong Id,
    string Name,
    PostKind Kind,
    int X,
    int Y,
    int Width,
    int Height,
    int Rotation,
    bool IsCritical,
    decimal? TargetPerHour,
    string? Operation,
    RunAllocationResponse? Allocation,
    double OccupiedMinutes,
    int Produced,
    decimal? Expected,
    decimal? Attainment);
