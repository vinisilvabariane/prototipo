using MeOne.Domain.Enums;

namespace MeOne.Domain.Rules;

public static class ScopeHierarchy
{
    public static ScopeType? ParentTypeOf(ScopeType type) => type switch
    {
        ScopeType.Area => ScopeType.Plant,
        ScopeType.Line => ScopeType.Area,
        ScopeType.Workstation => ScopeType.Line,
        _ => null,
    };
}
