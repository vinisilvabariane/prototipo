using MeOne.Domain.Enums;

namespace MeOne.Domain.Entities;

public sealed record RunPostSnapshot(
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
    string? Operation);

public sealed record RunDecorationSnapshot(
    DecorationKind Kind,
    int X,
    int Y,
    int Width,
    int Height,
    int Rotation,
    string? Label);

public sealed record RunSnapshot(IReadOnlyList<RunPostSnapshot> Posts, IReadOnlyList<RunDecorationSnapshot> Decorations)
{
    public static RunSnapshot Empty { get; } = new([], []);

    public static RunSnapshot From(LineLayout layout) => new(
        [
            .. layout.Posts.Select(post => new RunPostSnapshot(
                post.Id, post.Name, post.Kind, post.X, post.Y, post.Width, post.Height, post.Rotation, post.IsCritical, post.TargetPerHour, post.Operation)),
        ],
        [
            .. layout.Decorations.Select(decoration => new RunDecorationSnapshot(
                decoration.Kind, decoration.X, decoration.Y, decoration.Width, decoration.Height, decoration.Rotation, decoration.Label)),
        ]);

    public RunPostSnapshot? FindPost(ulong id) => Posts.FirstOrDefault(post => post.Id == id);
}
