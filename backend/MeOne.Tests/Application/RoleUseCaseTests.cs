using MeOne.Application.Communication.Roles.Requests;
using MeOne.Application.Errors;
using MeOne.Application.UseCases.Roles;
using MeOne.Application.UseCases.Users;
using MeOne.Domain.Errors;
using MeOne.Domain.Rules;
using MeOne.Tests.Fakes;

namespace MeOne.Tests.Application;

public sealed class RoleUseCaseTests
{
    private static AssignRoleUseCase Assign(AccessFixture f, FakeCurrentActor actor) =>
        new(f.Users, f.Roles, f.Scopes, f.AccessProfiles, actor);

    private static RevokeRoleUseCase Revoke(AccessFixture f, FakeCurrentActor actor) =>
        new(f.Users, f.Scopes, f.AccessProfiles, actor);

    [Fact]
    public async Task Catalog_SeedsFunctionalRolesWithPermissions()
    {
        var f = new AccessFixture();

        Assert.Equal(RoleCatalog.All.Count, f.Roles.Stored.Count);
        Assert.Contains(Permissions.ProductionExecute, f.OperatorRole.PermissionCodes);
        Assert.DoesNotContain(Permissions.UsersManage, f.OperatorRole.PermissionCodes);
        Assert.Equal(Permissions.All.Count, f.MasterRole.PermissionCodes.Count);

        var response = await new GetRoleUseCase(f.Roles).ExecuteAsync(f.SupervisorRole.Id, CancellationToken.None);
        Assert.Contains(Permissions.ProductionSupervise, response.Value.Permissions);
    }

    [Fact]
    public async Task Create_WithPermissionsTheActorHas_Succeeds()
    {
        var f = new AccessFixture();

        var result = await new CreateRoleUseCase(f.Roles, FakeCurrentActor.Administrator()).ExecuteAsync(
            new CreateRoleRequest("logistica", "Logística", [Permissions.ProductionRead]),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal([Permissions.ProductionRead], result.Value.Permissions);
    }

    [Fact]
    public async Task Create_UnknownPermission_Fails()
    {
        var f = new AccessFixture();

        var result = await new CreateRoleUseCase(f.Roles, FakeCurrentActor.Administrator()).ExecuteAsync(
            new CreateRoleRequest("logistica", "Logística", ["warehouse.fly"]),
            CancellationToken.None);

        Assert.Equal(RoleErrors.UnknownPermission("warehouse.fly"), Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Create_WithPermissionTheActorLacks_Forbidden()
    {
        var f = new AccessFixture();
        var actor = FakeCurrentActor.WithPermissions(5, Permissions.RolesManage);

        var result = await new CreateRoleUseCase(f.Roles, actor).ExecuteAsync(
            new CreateRoleRequest("super", "Escalada", [Permissions.UsersManage]),
            CancellationToken.None);

        Assert.True(result.HasError<ForbiddenError>());
    }

    [Fact]
    public async Task Create_NamedMaster_Rejects()
    {
        var f = new AccessFixture();

        var result = await new CreateRoleUseCase(f.Roles, FakeCurrentActor.Administrator()).ExecuteAsync(
            new CreateRoleRequest("Master", "Outro admin", []),
            CancellationToken.None);

        Assert.Equal(RoleErrors.MasterRoleProtected, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Update_RenamingToMaster_Rejects()
    {
        var f = new AccessFixture();

        var result = await new UpdateRoleUseCase(f.Roles).ExecuteAsync(
            f.OperatorRole.Id,
            new UpdateRoleRequest("master", "Virou admin"),
            CancellationToken.None);

        Assert.Equal(RoleErrors.MasterRoleProtected, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Update_RenamingMaster_Rejects()
    {
        var f = new AccessFixture();

        var result = await new UpdateRoleUseCase(f.Roles).ExecuteAsync(
            f.MasterRole.Id,
            new UpdateRoleRequest("admin", "Administração"),
            CancellationToken.None);

        Assert.Equal(RoleErrors.MasterRoleProtected, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task SetPermissions_ReplacesAndInvalidatesEveryProfile()
    {
        var f = new AccessFixture();
        var useCase = new SetRolePermissionsUseCase(f.Roles, f.AccessProfiles, FakeCurrentActor.Administrator());

        var result = await useCase.ExecuteAsync(
            f.OperatorRole.Id,
            new SetRolePermissionsRequest([Permissions.ProductionRead]),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal([Permissions.ProductionRead], f.OperatorRole.PermissionCodes);
        Assert.Equal(1, f.AccessProfiles.InvalidatedAll);
    }

    [Fact]
    public async Task SetPermissions_OnMaster_Rejects()
    {
        var f = new AccessFixture();
        var useCase = new SetRolePermissionsUseCase(f.Roles, f.AccessProfiles, FakeCurrentActor.Administrator());

        var result = await useCase.ExecuteAsync(
            f.MasterRole.Id,
            new SetRolePermissionsRequest([]),
            CancellationToken.None);

        Assert.Equal(RoleErrors.MasterRoleProtected, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task SetPermissions_AddingPermissionTheActorLacks_Forbidden()
    {
        var f = new AccessFixture();
        var actor = FakeCurrentActor.WithPermissions(5, Permissions.RolesManage, Permissions.ProductionRead);
        var useCase = new SetRolePermissionsUseCase(f.Roles, f.AccessProfiles, actor);

        var result = await useCase.ExecuteAsync(
            f.OperatorRole.Id,
            new SetRolePermissionsRequest([.. f.OperatorRole.PermissionCodes, Permissions.UsersManage]),
            CancellationToken.None);

        Assert.True(result.HasError<ForbiddenError>());
    }

    [Fact]
    public async Task Assign_GlobalRole_GrantsAndInvalidatesProfile()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);

        var result = await Assign(f, FakeCurrentActor.Administrator())
            .ExecuteAsync(new AssignRoleRequest(user.Id, f.OperatorRole.Id, null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(user.HasRole(f.OperatorRole.Id, null));
        Assert.Contains(user.Id, f.AccessProfiles.Invalidated);
    }

    [Fact]
    public async Task Assign_ScopedRole_AppliesToChildScopesOnly()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);

        await Assign(f, FakeCurrentActor.Administrator())
            .ExecuteAsync(new AssignRoleRequest(user.Id, f.SupervisorRole.Id, f.AreaA.Id), CancellationToken.None);

        var profile = AccessProfile.From(user);
        var lineChain = await f.Scopes.GetChainAsync(f.LineA1.Id, CancellationToken.None);
        var otherAreaChain = await f.Scopes.GetChainAsync(f.AreaB.Id, CancellationToken.None);

        Assert.True(profile.HasIn(Permissions.ProductionSupervise, lineChain));
        Assert.False(profile.HasIn(Permissions.ProductionSupervise, otherAreaChain));
        Assert.False(profile.HasGlobal(Permissions.ProductionSupervise));
    }

    [Fact]
    public async Task Assign_SameRoleInTwoScopes_IsAllowedButNotTwiceInTheSame()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        var useCase = Assign(f, FakeCurrentActor.Administrator());

        var areaA = await useCase.ExecuteAsync(new AssignRoleRequest(user.Id, f.OperatorRole.Id, f.AreaA.Id), CancellationToken.None);
        var areaB = await useCase.ExecuteAsync(new AssignRoleRequest(user.Id, f.OperatorRole.Id, f.AreaB.Id), CancellationToken.None);
        var again = await useCase.ExecuteAsync(new AssignRoleRequest(user.Id, f.OperatorRole.Id, f.AreaA.Id), CancellationToken.None);

        Assert.True(areaA.IsSuccess);
        Assert.True(areaB.IsSuccess);
        Assert.Equal(RoleErrors.AlreadyAssigned, Assert.Single(again.Errors).Message);
    }

    [Fact]
    public async Task Assign_MasterWithScope_Rejects()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);

        var result = await Assign(f, FakeCurrentActor.Administrator())
            .ExecuteAsync(new AssignRoleRequest(user.Id, f.MasterRole.Id, f.Plant.Id), CancellationToken.None);

        Assert.Equal(RoleErrors.MasterMustBeGlobal, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Assign_AdministrativeRoleWithScope_Rejects()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);

        var result = await Assign(f, FakeCurrentActor.Administrator())
            .ExecuteAsync(new AssignRoleRequest(user.Id, f.AccessManagerRole.Id, f.Plant.Id), CancellationToken.None);

        Assert.Equal(RoleErrors.GlobalOnlyPermissionInScope, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Assign_AccessManagerCannotGrantMaster()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        var manager = f.AddUser(54321, "manager@grupomultilaser.com.br", roles: f.AccessManagerRole);

        var result = await Assign(f, FakeCurrentActor.For(manager))
            .ExecuteAsync(new AssignRoleRequest(user.Id, f.MasterRole.Id, null), CancellationToken.None);

        Assert.True(result.HasError<ForbiddenError>());
        Assert.False(user.IsAdministrator);
    }

    [Fact]
    public async Task Assign_AccessManagerCannotGrantRoleWithPermissionsItLacks()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        var manager = f.AddUser(54321, "manager@grupomultilaser.com.br", roles: f.AccessManagerRole);

        var result = await Assign(f, FakeCurrentActor.For(manager))
            .ExecuteAsync(new AssignRoleRequest(user.Id, f.OperatorRole.Id, f.LineA1.Id), CancellationToken.None);

        Assert.True(result.HasError<ForbiddenError>());
    }

    [Fact]
    public async Task Assign_ScopedSupervisorGrantsOperatorOnlyInsideOwnArea()
    {
        var f = new AccessFixture();
        var supervisor = f.AddUser(54321, "supervisor@grupomultilaser.com.br");
        supervisor.GrantRole(f.SupervisorRole, f.AreaA);
        var target = f.AddUser(12345);
        var actor = FakeCurrentActor.For(supervisor);

        var inside = await Assign(f, actor)
            .ExecuteAsync(new AssignRoleRequest(target.Id, f.OperatorRole.Id, f.LineA1.Id), CancellationToken.None);
        var outside = await Assign(f, actor)
            .ExecuteAsync(new AssignRoleRequest(target.Id, f.OperatorRole.Id, f.AreaB.Id), CancellationToken.None);

        Assert.True(inside.IsSuccess);
        Assert.True(outside.HasError<ForbiddenError>());
    }

    [Fact]
    public async Task Assign_UnknownScope_NotFound()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);

        var result = await Assign(f, FakeCurrentActor.Administrator())
            .ExecuteAsync(new AssignRoleRequest(user.Id, f.OperatorRole.Id, 9999), CancellationToken.None);

        Assert.True(result.HasError<NotFoundError>());
    }

    [Fact]
    public async Task Revoke_RemovesOnlyTheGivenScope()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        user.GrantRole(f.OperatorRole, f.AreaA);
        user.GrantRole(f.OperatorRole, f.AreaB);

        var result = await Revoke(f, FakeCurrentActor.Administrator())
            .ExecuteAsync(new RevokeRoleRequest(user.Id, f.OperatorRole.Id, f.AreaA.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(user.HasRole(f.OperatorRole.Id, f.AreaA.Id));
        Assert.True(user.HasRole(f.OperatorRole.Id, f.AreaB.Id));

        var roles = await new ListUserRolesUseCase(f.Users).ExecuteAsync(user.Id, CancellationToken.None);
        Assert.Equal("AREA-B", Assert.Single(roles.Value).ScopeCode);
    }

    [Fact]
    public async Task Revoke_OwnMaster_Rejects()
    {
        var f = new AccessFixture();
        var admin = f.AddAdministrator();
        f.AddAdministrator(9002);

        var result = await Revoke(f, FakeCurrentActor.For(admin))
            .ExecuteAsync(new RevokeRoleRequest(admin.Id, f.MasterRole.Id, null), CancellationToken.None);

        Assert.Equal(RoleErrors.CannotRevokeOwnMaster, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Revoke_LastAdministrator_Rejects()
    {
        var f = new AccessFixture();
        var admin = f.AddAdministrator();

        var result = await Revoke(f, FakeCurrentActor.Administrator())
            .ExecuteAsync(new RevokeRoleRequest(admin.Id, f.MasterRole.Id, null), CancellationToken.None);

        Assert.Equal(UserErrors.LastAdministrator, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Revoke_NotAssigned_Fails()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);

        var result = await Revoke(f, FakeCurrentActor.Administrator())
            .ExecuteAsync(new RevokeRoleRequest(user.Id, f.OperatorRole.Id, null), CancellationToken.None);

        Assert.Equal(RoleErrors.NotAssigned, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Delete_RoleInUse_Rejects()
    {
        var f = new AccessFixture();
        f.AddUser(12345, roles: f.OperatorRole).Deactivate(modifiedBy: 1);

        var result = await new DeleteRoleUseCase(f.Roles).ExecuteAsync(f.OperatorRole.Id, CancellationToken.None);

        Assert.Equal(RoleErrors.InUse, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Delete_Master_Rejects()
    {
        var f = new AccessFixture();

        var result = await new DeleteRoleUseCase(f.Roles).ExecuteAsync(f.MasterRole.Id, CancellationToken.None);

        Assert.Equal(RoleErrors.MasterRoleProtected, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task List_TotalIsTableTotal()
    {
        var f = new AccessFixture();

        var result = await new ListRolesUseCase(f.Roles).ExecuteAsync(new RolesQuery { Page = 0, PageSize = 1 }, CancellationToken.None);

        Assert.Equal(RoleCatalog.All.Count, result.Value.TotalCount);
        Assert.Single(result.Value.Items);
    }

    [Fact]
    public async Task ListPermissions_ReturnsTheCatalog()
    {
        var result = await new ListPermissionsUseCase().ExecuteAsync(CancellationToken.None);

        Assert.Equal(Permissions.All.Count, result.Value.Count);
        Assert.Contains(result.Value, permission => permission.Code == Permissions.UsersManage && permission.GlobalOnly);
    }
}
