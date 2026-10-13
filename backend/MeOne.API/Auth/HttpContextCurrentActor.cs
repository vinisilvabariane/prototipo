using System.Globalization;
using System.Security.Claims;
using MeOne.Application.Interfaces;

namespace MeOne.API.Auth;

public sealed class HttpContextCurrentActor : ICurrentActor
{
    private readonly IHttpContextAccessor _accessor;

    public HttpContextCurrentActor(IHttpContextAccessor accessor) => _accessor = accessor;

    public string? Registration
    {
        get
        {
            var user = _accessor.HttpContext?.User;

            if (user is null)
                return null;

            return user.FindFirstValue("registration") ??
                   user.FindFirstValue("sub") ??
                   user.FindFirstValue(ClaimTypes.NameIdentifier) ??
                   user.Identity?.Name;
        }
    }

    public ulong? UserId
    {
        get
        {
            var user = _accessor.HttpContext?.User;

            if (user?.Identity?.IsAuthenticated != true)
                return null;

            var value = user.FindFirstValue("sub") ?? user.FindFirstValue(ClaimTypes.NameIdentifier);

            return ulong.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var id) ? id : null;
        }
    }

    public bool IsInRole(string role) => _accessor.HttpContext?.User.IsInRole(role) ?? false;
}
