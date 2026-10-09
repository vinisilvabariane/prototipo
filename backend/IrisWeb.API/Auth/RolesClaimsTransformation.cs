using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;

namespace IrisWeb.API.Auth;

public sealed class RolesClaimsTransformation : IClaimsTransformation
{
    private const string RolesClaim = "roles";

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
            return Task.FromResult(principal);

        foreach (var claim in identity.FindAll(RolesClaim).ToList())
        {
            foreach (var role in ParseRoles(claim.Value))
            {
                if (!identity.HasClaim(ClaimTypes.Role, role))
                    identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }
        }

        return Task.FromResult(principal);
    }

    private static IEnumerable<string> ParseRoles(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return [];

        if (!value.TrimStart().StartsWith('['))
            return [value];

        try
        {
            return JsonSerializer.Deserialize<List<string>>(value) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
