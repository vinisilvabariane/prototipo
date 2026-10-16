using MeOne.API.Auth;
using MeOne.Domain.Rules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace MeOne.Tests.API;

public sealed class PermissionPolicyProviderTests
{
    private static PermissionPolicyProvider Provider() => new(Options.Create(new AuthorizationOptions()));

    [Fact]
    public async Task PermissionPolicy_RequiresAuthenticatedUserAndThePermission()
    {
        var policy = await Provider().GetPolicyAsync(RequirePermissionAttribute.PolicyFor(Permissions.UsersManage));

        Assert.NotNull(policy);
        Assert.Contains(policy.Requirements, requirement => requirement is PermissionRequirement { Permission: Permissions.UsersManage });
        Assert.Contains(policy.Requirements, requirement => requirement is Microsoft.AspNetCore.Authorization.Infrastructure.DenyAnonymousAuthorizationRequirement);
    }

    [Fact]
    public async Task OtherPolicies_FallBackToTheDefaultProvider()
    {
        var policy = await Provider().GetPolicyAsync("unknown-policy");

        Assert.Null(policy);
    }
}
