using MeOne.Domain.Enums;

namespace MeOne.Domain.Entities;

public sealed record LayoutPostDraft(
    long Id,
    string Name,
    PostKind Kind,
    int X,
    int Y,
    int Width,
    int Height,
    int Rotation,
    bool IsCritical,
    decimal? TargetPerHour,
    string? Operation);

public sealed record LayoutDecorationDraft(
    long Id,
    DecorationKind Kind,
    int X,
    int Y,
    int Width,
    int Height,
    int Rotation,
    string? Label,
    bool Locked);
