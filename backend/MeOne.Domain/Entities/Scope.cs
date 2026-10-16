using MeOne.Domain.Enums;

namespace MeOne.Domain.Entities;

public sealed class Scope : Entity<ulong>
{
    private Scope() { }

    public ScopeType Type { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public ulong? ParentId { get; private set; }

    public static Scope Create(ScopeType type, string code, string name, ulong? parentId)
    {
        var scope = new Scope { Type = type, ParentId = parentId };
        scope.Update(code, name);
        return scope;
    }

    public bool Update(string code, string name)
    {
        var normalizedCode = NormalizeCode(code);
        var trimmedName = name.Trim();

        if (Code == normalizedCode && Name == trimmedName)
            return false;

        Code = normalizedCode;
        Name = trimmedName;
        return true;
    }

    public static string NormalizeCode(string code) => code.Trim().ToUpperInvariant();
}
