using System.Security.Claims;
using MeOne.Application.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.API.Auth;

public sealed class HttpContextCurrentActor(IHttpContextAccessor accessor, AccessProfileCache accessProfiles) : ICurrentActor
{
    public string? Registration
    {
        get
        {
            var user = accessor.HttpContext?.User;

            if (user?.Identity?.IsAuthenticated != true)
                return null;

            return user.FindFirstValue(TokenClaims.Registration) ??
                   user.FindFirstValue(TokenClaims.Subject) ??
                   user.FindFirstValue(ClaimTypes.NameIdentifier) ??
                   user.Identity?.Name;
        }
    }

    public ulong? UserId
    {
        get
        {
            var user = accessor.HttpContext?.User;

            return user?.Identity?.IsAuthenticated == true ? TokenClaims.UserId(user) : null;
        }
    }

    public async Task<AccessProfile?> GetAccessProfileAsync(CancellationToken cancellationToken) =>
        UserId is { } userId ? await accessProfiles.GetAsync(userId, cancellationToken) : null;
}
