using MeOne.Domain.Rules;
using MeOne.Tests.Fakes;

namespace MeOne.Tests.Domain;

public sealed class AccessProfileTests
{
    private static AccessProfile Profile(params AccessGrant[] grants) =>
        new(1, active: true, "stamp", isAdministrator: false, grants);

    [Fact]
    public void Administrator_HasEveryPermissionEverywhere()
    {
        var admin = new AccessProfile(1, active: true, "stamp", isAdministrator: true, []);

        Assert.All(Permissions.All, permission => Assert.True(admin.HasGlobal(permission.Code)));
    }

    [Fact]
    public void GlobalOnlyPermission_GrantedInScope_DoesNotAllow()
    {
        var profile = Profile(new AccessGrant(Permissions.UsersManage, 10));

        Assert.False(profile.Allows(Permissions.UsersManage));
    }

    [Fact]
    public void ScopedPermission_GrantedInAnyScope_AllowsEndpoint()
    {
        var profile = Profile(new AccessGrant(Permissions.ProductionExecute, 10));

        Assert.True(profile.Allows(Permissions.ProductionExecute));
        Assert.False(profile.HasGlobal(Permissions.ProductionExecute));
    }

    [Fact]
    public void HasIn_InheritsFromAncestorScopes()
    {
        var profile = Profile(new AccessGrant(Permissions.ProductionExecute, 10));

        Assert.True(profile.HasIn(Permissions.ProductionExecute, [30, 20, 10]));
        Assert.False(profile.HasIn(Permissions.ProductionExecute, [40, 20]));
    }

    [Fact]
    public void Dominates_RequiresEveryTargetPermissionGlobally()
    {
        var manager = Profile(new AccessGrant(Permissions.UsersManage, null), new AccessGrant(Permissions.ProductionRead, null));
        var reader = Profile(new AccessGrant(Permissions.ProductionRead, 10));
        var executor = Profile(new AccessGrant(Permissions.ProductionExecute, 10));
        var admin = new AccessProfile(2, active: true, "stamp", isAdministrator: true, []);

        Assert.True(manager.Dominates(reader));
        Assert.False(manager.Dominates(executor));
        Assert.False(manager.Dominates(admin));
        Assert.True(admin.Dominates(manager));
    }

    [Fact]
    public void From_User_UsesRolePermissionsAndScopes()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        user.GrantRole(f.OperatorRole, f.LineA1);

        var profile = AccessProfile.From(user);

        Assert.False(profile.IsAdministrator);
        Assert.Contains(new AccessGrant(Permissions.ProductionExecute, f.LineA1.Id), profile.Grants);
        Assert.DoesNotContain(profile.Grants, grant => grant.ScopeId is null);
    }

    [Fact]
    public void User_LockoutAfterMaxFailures_AndUnlocksAfterDuration()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        var now = DateTime.UtcNow;

        for (var attempt = 0; attempt < LoginLockoutPolicy.MaxFailedAttempts; attempt++)
            user.RegisterFailedLogin(now);

        Assert.True(user.IsLockedOut(now));
        Assert.False(user.IsLockedOut(now.Add(LoginLockoutPolicy.LockoutDuration).AddSeconds(1)));
    }

    [Fact]
    public void User_PasswordChange_RotatesSecurityStampAndClearsLockout()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        var stamp = user.SecurityStamp;
        for (var attempt = 0; attempt < LoginLockoutPolicy.MaxFailedAttempts; attempt++)
            user.RegisterFailedLogin(DateTime.UtcNow);

        user.ChangePassword("hash:new", modifiedBy: 1);

        Assert.NotEqual(stamp, user.SecurityStamp);
        Assert.False(user.IsLockedOut(DateTime.UtcNow));
    }
}
