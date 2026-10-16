using Microsoft.AspNetCore.Authorization;

namespace MeOne.API.Auth;

public sealed class RequirePermissionAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "permission:";

    public RequirePermissionAttribute(string permission) : base(PolicyFor(permission))
    {
        Permission = permission;
    }

    public string Permission { get; }

    public static string PolicyFor(string permission) => PolicyPrefix + permission;
}
