using MeOne.Domain.Entities;

namespace MeOne.Domain.Rules;

public static class OrgChartHierarchy
{
    public const int MaxDepth = 12;

    public static int DepthOf(IReadOnlyCollection<OrgPosition> positions, ulong? positionId)
    {
        var byId = positions.ToDictionary(position => position.Id);
        var depth = 0;
        var current = positionId;

        while (current is { } id && byId.TryGetValue(id, out var position) && depth <= positions.Count)
        {
            depth++;
            current = position.ParentId;
        }

        return depth;
    }

    public static int HeightOf(IReadOnlyCollection<OrgPosition> positions, ulong positionId)
    {
        var children = positions.ToLookup(position => position.ParentId);

        int Walk(ulong id, int guard) => guard > positions.Count
            ? 0
            : 1 + children[id].Select(child => Walk(child.Id, guard + 1)).DefaultIfEmpty(0).Max();

        return Walk(positionId, 0);
    }

    public static bool IsSelfOrBelow(IReadOnlyCollection<OrgPosition> positions, ulong ancestorId, ulong? candidateId)
    {
        var byId = positions.ToDictionary(position => position.Id);
        var current = candidateId;
        var steps = 0;

        while (current is { } id && steps++ <= positions.Count)
        {
            if (id == ancestorId)
                return true;

            current = byId.TryGetValue(id, out var position) ? position.ParentId : null;
        }

        return false;
    }
}
