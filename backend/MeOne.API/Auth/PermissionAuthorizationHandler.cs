using Microsoft.AspNetCore.Authorization;

namespace MeOne.API.Auth;

public sealed class PermissionAuthorizationHandler(AccessProfileCache accessProfiles, IHttpContextAccessor accessor)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (TokenClaims.UserId(context.User) is not { } userId)
            return;

        var profile = await accessProfiles.GetAsync(userId, accessor.HttpContext?.RequestAborted ?? CancellationToken.None);

        if (profile is { Active: true } && profile.Allows(requirement.Permission))
            context.Succeed(requirement);
    }
}
