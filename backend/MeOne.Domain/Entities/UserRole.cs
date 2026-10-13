namespace MeOne.Domain.Entities;

public sealed class UserRole : Entity<ulong>
{
    private UserRole() { }

    public ulong UserId { get; private set; }

    public ulong RoleId { get; private set; }

    public Role? Role { get; private set; }

    internal static UserRole Link(ulong userId, Role role) => new()
    {
        UserId = userId,
        RoleId = role.Id,
        Role = role,
    };
}
