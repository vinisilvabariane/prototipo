namespace MeOne.Domain.Entities;

public sealed class UserRole : Entity<ulong>
{
    private UserRole() { }

    public ulong UserId { get; private set; }

    public ulong RoleId { get; private set; }

    public ulong? ScopeId { get; private set; }

    public Role? Role { get; private set; }

    public Scope? Scope { get; private set; }

    internal static UserRole Link(ulong userId, Role role, Scope? scope) => new()
    {
        UserId = userId,
        RoleId = role.Id,
        Role = role,
        ScopeId = scope?.Id,
        Scope = scope,
    };
}
