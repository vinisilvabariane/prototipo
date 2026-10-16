using MeOne.Domain.Enums;

namespace MeOne.Domain.Entities;

public sealed class LayoutPost : Entity<ulong>
{
    private LayoutPost() { }

    public ulong LayoutId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public PostKind Kind { get; private set; }

    public int X { get; private set; }

    public int Y { get; private set; }

    public int Width { get; private set; }

    public int Height { get; private set; }

    public int Rotation { get; private set; }

    public bool IsCritical { get; private set; }

    public decimal? TargetPerHour { get; private set; }

    public string? Operation { get; private set; }

    internal static LayoutPost Create(LayoutPostDraft draft)
    {
        var post = new LayoutPost();
        post.Apply(draft);
        return post;
    }

    internal void Apply(LayoutPostDraft draft)
    {
        Name = draft.Name.Trim();
        Kind = draft.Kind;
        X = draft.X;
        Y = draft.Y;
        Width = draft.Width;
        Height = draft.Height;
        Rotation = draft.Rotation;
        IsCritical = draft.IsCritical;
        TargetPerHour = draft.TargetPerHour;
        Operation = string.IsNullOrWhiteSpace(draft.Operation) ? null : draft.Operation.Trim();
    }

    internal LayoutPostDraft ToDraft() => new(0, Name, Kind, X, Y, Width, Height, Rotation, IsCritical, TargetPerHour, Operation);
}
