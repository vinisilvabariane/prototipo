using MeOne.Domain.Enums;

namespace MeOne.Domain.Entities;

public sealed class LayoutDecoration : Entity<ulong>
{
    private LayoutDecoration() { }

    public ulong LayoutId { get; private set; }

    public DecorationKind Kind { get; private set; }

    public int X { get; private set; }

    public int Y { get; private set; }

    public int Width { get; private set; }

    public int Height { get; private set; }

    public int Rotation { get; private set; }

    public string? Label { get; private set; }

    public bool Locked { get; private set; }

    internal static LayoutDecoration Create(LayoutDecorationDraft draft)
    {
        var decoration = new LayoutDecoration();
        decoration.Apply(draft);
        return decoration;
    }

    internal void Apply(LayoutDecorationDraft draft)
    {
        Kind = draft.Kind;
        X = draft.X;
        Y = draft.Y;
        Width = draft.Width;
        Height = draft.Height;
        Rotation = draft.Rotation;
        Label = string.IsNullOrWhiteSpace(draft.Label) ? null : draft.Label.Trim();
        Locked = draft.Locked;
    }

    internal LayoutDecorationDraft ToDraft() => new(0, Kind, X, Y, Width, Height, Rotation, Label, Locked);
}
