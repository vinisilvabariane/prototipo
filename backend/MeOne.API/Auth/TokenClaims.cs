using System.Globalization;
using System.Security.Claims;

namespace MeOne.API.Auth;

public static class TokenClaims
{
    public const string Subject = "sub";
    public const string Registration = "registration";
    public const string SecurityStamp = "sstamp";
    public const string Name = "name";
    public const string Roles = "roles";

    public static ulong? UserId(ClaimsPrincipal? principal)
    {
        var value = principal?.FindFirstValue(Subject) ?? principal?.FindFirstValue(ClaimTypes.NameIdentifier);

        return ulong.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var id) ? id : null;
    }
}
