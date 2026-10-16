using MeOne.Domain.Rules;

namespace MeOne.Domain.Entities;

public sealed class Role : Entity<ulong>
{
    private readonly List<RolePermission> _permissions = [];

    private Role() { }

    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public IReadOnlyCollection<RolePermission> Permissions => _permissions;

    public bool IsAdministrator => AccessRoles.IsMaster(Name);

    public IReadOnlyList<string> PermissionCodes => IsAdministrator
        ? [.. Rules.Permissions.All.Select(permission => permission.Code)]
        : [.. _permissions.Select(permission => permission.PermissionCode).Order(StringComparer.Ordinal)];

    public static Role Create(string name, string description, IEnumerable<string>? permissionCodes = null)
    {
        var role = new Role();
        role.Update(name, description);
        role.SetPermissions(permissionCodes ?? []);
        return role;
    }

    public bool Update(string name, string description)
    {
        var trimmedName = name.Trim();
        var trimmedDescription = description.Trim();

        if (Name == trimmedName && Description == trimmedDescription)
            return false;

        Name = trimmedName;
        Description = trimmedDescription;
        return true;
    }

    public bool SetPermissions(IEnumerable<string> permissionCodes)
    {
        var wanted = permissionCodes.Select(code => code.Trim()).ToHashSet(StringComparer.Ordinal);
        var current = _permissions.Select(permission => permission.PermissionCode).ToHashSet(StringComparer.Ordinal);

        if (wanted.SetEquals(current))
            return false;

        _permissions.RemoveAll(permission => !wanted.Contains(permission.PermissionCode));

        foreach (var code in wanted.Where(code => !current.Contains(code)))
            _permissions.Add(RolePermission.Link(Id, code));

        return true;
    }
}
