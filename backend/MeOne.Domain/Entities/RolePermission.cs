namespace MeOne.Domain.Entities;

public sealed class RolePermission : Entity<ulong>
{
    private RolePermission() { }

    public ulong RoleId { get; private set; }

    public string PermissionCode { get; private set; } = string.Empty;

    internal static RolePermission Link(ulong roleId, string permissionCode) => new()
    {
        RoleId = roleId,
        PermissionCode = permissionCode,
    };
}
