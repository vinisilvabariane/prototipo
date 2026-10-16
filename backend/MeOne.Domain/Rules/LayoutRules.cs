using MeOne.Domain.Entities;
using MeOne.Domain.Enums;
using MeOne.Domain.Errors;

namespace MeOne.Domain.Rules;

public static class LayoutRules
{
    public const int MaxPosts = 200;
    public const int MaxDecorations = 200;
    public const int MaxCoordinate = 50_000;
    public const int MinPostSize = 40;
    public const int MaxPostSize = 2_000;
    public const int MinDecorationSize = 10;
    public const int MaxDecorationSize = 6_000;
    public const decimal MaxTargetPerHour = 100_000;

    public static string? Check(IReadOnlyList<LayoutPostDraft> posts, IReadOnlyList<LayoutDecorationDraft> decorations)
    {
        if (posts.Count > MaxPosts)
            return ProductionErrors.TooManyPosts;

        if (decorations.Count > MaxDecorations)
            return ProductionErrors.TooManyDecorations;

        var names = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
        var ids = new HashSet<long>();

        foreach (var post in posts)
        {
            if (string.IsNullOrWhiteSpace(post.Name))
                return ProductionErrors.PostNameRequired;

            if (post.Name.Trim().Length > ProductionErrors.MaxPostNameLength)
                return ProductionErrors.FieldTooLong("O nome do posto", ProductionErrors.MaxPostNameLength);

            if (!names.Add(post.Name.Trim()))
                return ProductionErrors.PostNameTaken(post.Name.Trim());

            if (post.Id > 0 && !ids.Add(post.Id))
                return ProductionErrors.PostRepeated;

            if (!Enum.IsDefined(post.Kind))
                return ProductionErrors.PostKindInvalid;

            if (!IsInside(post.X, post.Y) || post.Width is < MinPostSize or > MaxPostSize || post.Height is < MinPostSize or > MaxPostSize)
                return ProductionErrors.PostGeometryInvalid;

            if (post.Rotation is < 0 or > 359)
                return ProductionErrors.RotationInvalid;

            if (post.TargetPerHour is < 0 or > MaxTargetPerHour)
                return ProductionErrors.TargetInvalid;

            if (post.Operation is { } operation && operation.Trim().Length > ProductionErrors.MaxOperationLength)
                return ProductionErrors.FieldTooLong("A operação", ProductionErrors.MaxOperationLength);
        }

        var decorationIds = new HashSet<long>();

        foreach (var decoration in decorations)
        {
            if (decoration.Id > 0 && !decorationIds.Add(decoration.Id))
                return ProductionErrors.DecorationRepeated;

            if (!Enum.IsDefined(decoration.Kind))
                return ProductionErrors.DecorationKindInvalid;

            if (!IsInside(decoration.X, decoration.Y) || decoration.Width is < MinDecorationSize or > MaxDecorationSize || decoration.Height is < MinDecorationSize or > MaxDecorationSize)
                return ProductionErrors.DecorationGeometryInvalid;

            if (decoration.Rotation is < 0 or > 359)
                return ProductionErrors.RotationInvalid;

            if (decoration.Label is { } label && label.Trim().Length > ProductionErrors.MaxLabelLength)
                return ProductionErrors.FieldTooLong("O texto da decoração", ProductionErrors.MaxLabelLength);
        }

        return null;
    }

    private static bool IsInside(int x, int y) => Math.Abs(x) <= MaxCoordinate && Math.Abs(y) <= MaxCoordinate;
}
