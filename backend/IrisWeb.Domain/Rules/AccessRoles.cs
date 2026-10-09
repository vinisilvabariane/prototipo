namespace IrisWeb.Domain.Rules;

public static class AccessRoles
{
    /// <summary>Role de administração: governa usuários, roles, departamentos e o catálogo de modelos.</summary>
    public const string Master = "master";

    public static bool IsMaster(string roleName) =>
        string.Equals(roleName.Trim(), Master, StringComparison.OrdinalIgnoreCase);
}
