namespace MeOne.Domain.Rules;

public static class AccessRoles
{
    public const string Master = "master";

    public static bool IsMaster(string roleName) =>
        string.Equals(roleName.Trim(), Master, StringComparison.OrdinalIgnoreCase);
}
