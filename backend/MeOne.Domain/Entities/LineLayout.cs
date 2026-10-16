using MeOne.Domain.Errors;
using MeOne.Domain.Rules;

namespace MeOne.Domain.Entities;

public sealed class LineLayout : Entity<ulong>
{
    private readonly List<LayoutPost> _posts = [];

    private readonly List<LayoutDecoration> _decorations = [];

    private LineLayout() { }

    public ulong LineId { get; private set; }

    public ulong ProductId { get; private set; }

    public ulong ShiftId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime UpdatedAt { get; private set; }

    public ulong UpdatedBy { get; private set; }

    public IReadOnlyList<LayoutPost> Posts => _posts;

    public IReadOnlyList<LayoutDecoration> Decorations => _decorations;

    public static LineLayout Create(ulong lineId, ulong productId, ulong shiftId, ulong userId, DateTime now) => new()
    {
        LineId = lineId,
        ProductId = productId,
        ShiftId = shiftId,
        CreatedAt = now,
        UpdatedAt = now,
        UpdatedBy = userId,
    };

    public LayoutPost? FindPost(ulong id) => _posts.FirstOrDefault(post => post.Id == id);

    public string? Replace(
        IReadOnlyList<LayoutPostDraft> posts,
        IReadOnlyList<LayoutDecorationDraft> decorations,
        ulong userId,
        DateTime now)
    {
        if (LayoutRules.Check(posts, decorations) is { } problem)
            return problem;

        var currentPosts = _posts.ToDictionary(post => (long)post.Id);
        var currentDecorations = _decorations.ToDictionary(decoration => (long)decoration.Id);

        if (posts.Any(draft => draft.Id > 0 && !currentPosts.ContainsKey(draft.Id)))
            return ProductionErrors.PostNotInLayout;

        if (decorations.Any(draft => draft.Id > 0 && !currentDecorations.ContainsKey(draft.Id)))
            return ProductionErrors.DecorationNotInLayout;

        var keptPosts = posts.Where(draft => draft.Id > 0).Select(draft => draft.Id).ToHashSet();
        var keptDecorations = decorations.Where(draft => draft.Id > 0).Select(draft => draft.Id).ToHashSet();

        _posts.RemoveAll(post => !keptPosts.Contains((long)post.Id));
        _decorations.RemoveAll(decoration => !keptDecorations.Contains((long)decoration.Id));

        foreach (var draft in posts)
        {
            if (draft.Id > 0)
                currentPosts[draft.Id].Apply(draft);
            else
                _posts.Add(LayoutPost.Create(draft));
        }

        foreach (var draft in decorations)
        {
            if (draft.Id > 0)
                currentDecorations[draft.Id].Apply(draft);
            else
                _decorations.Add(LayoutDecoration.Create(draft));
        }

        UpdatedBy = userId;
        UpdatedAt = now;
        return null;
    }

    public void CopyContentFrom(LineLayout source, ulong userId, DateTime now)
    {
        _posts.Clear();
        _decorations.Clear();

        _posts.AddRange(source._posts.Select(post => LayoutPost.Create(post.ToDraft())));
        _decorations.AddRange(source._decorations.Select(decoration => LayoutDecoration.Create(decoration.ToDraft())));

        UpdatedBy = userId;
        UpdatedAt = now;
    }
}
