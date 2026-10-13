using MeOne.Application.Communication.Roles.Requests;
using MeOne.Application.Errors;
using MeOne.Application.UseCases.Roles;
using MeOne.Application.UseCases.Users;
using MeOne.Domain.Errors;
using MeOne.Tests.Fakes;

namespace MeOne.Tests.Application;

public sealed class RoleUseCaseTests
{
    [Fact]
    public async Task Create_DuplicateNameIgnoringCase_Fails()
    {
        var f = new AccessFixture();

        var result = await new CreateRoleUseCase(f.Roles).ExecuteAsync(
            new CreateRoleRequest(" Operator ", "Outra"),
            CancellationToken.None);

        Assert.Equal(RoleErrors.NameTaken("Operator"), Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Create_ReturnsRoleWithId()
    {
        var f = new AccessFixture();

        var result = await new CreateRoleUseCase(f.Roles).ExecuteAsync(
            new CreateRoleRequest("quality", "Inspection"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(0UL, result.Value.Id);
    }

    [Fact]
    public async Task Update_RenamingMaster_Rejects()
    {
        var f = new AccessFixture();

        var result = await new UpdateRoleUseCase(f.Roles).ExecuteAsync(
            f.MasterRole.Id,
            new UpdateRoleRequest("admin", "Administration"),
            CancellationToken.None);

        Assert.Equal(RoleErrors.MasterRoleProtected, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Update_OnlyMasterDescription_IsAllowed()
    {
        var f = new AccessFixture();

        var result = await new UpdateRoleUseCase(f.Roles).ExecuteAsync(
            f.MasterRole.Id,
            new UpdateRoleRequest("master", "Governance"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Governance", f.MasterRole.Description);
    }

    [Fact]
    public async Task Update_WithoutChanges_SucceedsWithoutSaving()
    {
        var f = new AccessFixture();

        var result = await new UpdateRoleUseCase(f.Roles).ExecuteAsync(
            f.OperatorRole.Id,
            new UpdateRoleRequest(f.OperatorRole.Name, f.OperatorRole.Description),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, f.Roles.SaveCount);
    }

    [Fact]
    public async Task Delete_RoleInUse_EvenByDeactivatedUser_Rejects()
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
    public async Task Delete_Unknown_NotFound()
    {
        var f = new AccessFixture();

        var result = await new DeleteRoleUseCase(f.Roles).ExecuteAsync(999, CancellationToken.None);

        Assert.True(result.HasError<NotFoundError>());
    }

    [Fact]
    public async Task Assign_GrantsRoleAndRepeatingFails()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        var useCase = new AssignRoleUseCase(f.Users, f.Roles);

        var first = await useCase.ExecuteAsync(new AssignRoleRequest(user.Id, f.OperatorRole.Id), CancellationToken.None);
        var second = await useCase.ExecuteAsync(new AssignRoleRequest(user.Id, f.OperatorRole.Id), CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.Equal(["operator"], user.RoleNames);
        Assert.Equal(RoleErrors.AlreadyAssigned, Assert.Single(second.Errors).Message);
    }

    [Fact]
    public async Task Assign_UnknownRoleOrUser_NotFound()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        var useCase = new AssignRoleUseCase(f.Users, f.Roles);

        Assert.True((await useCase.ExecuteAsync(new AssignRoleRequest(user.Id, 999), CancellationToken.None))
            .HasError<NotFoundError>());
        Assert.True((await useCase.ExecuteAsync(new AssignRoleRequest(999, f.OperatorRole.Id), CancellationToken.None))
            .HasError<NotFoundError>());
    }

    [Fact]
    public async Task Revoke_RemovesRole()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345, roles: f.OperatorRole);

        var result = await new RevokeRoleUseCase(f.Users, FakeCurrentActor.Master(userId: 999))
            .ExecuteAsync(new RevokeRoleRequest(user.Id, f.OperatorRole.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(user.Roles);

        var roles = await new ListUserRolesUseCase(f.Users).ExecuteAsync(user.Id, CancellationToken.None);
        Assert.Empty(roles.Value);
    }

    [Fact]
    public async Task Revoke_MasterRemovingOwnMaster_Rejects()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345, roles: f.MasterRole);

        var result = await new RevokeRoleUseCase(f.Users, FakeCurrentActor.Master(userId: user.Id))
            .ExecuteAsync(new RevokeRoleRequest(user.Id, f.MasterRole.Id), CancellationToken.None);

        Assert.Equal(RoleErrors.CannotRevokeOwnMaster, Assert.Single(result.Errors).Message);
        Assert.True(user.HasRole(f.MasterRole.Id));
    }

    [Fact]
    public async Task Revoke_RoleUserDoesNotHave_Fails()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);

        var result = await new RevokeRoleUseCase(f.Users, FakeCurrentActor.Master())
            .ExecuteAsync(new RevokeRoleRequest(user.Id, f.OperatorRole.Id), CancellationToken.None);

        Assert.Equal(RoleErrors.NotAssigned, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task List_TotalIsTableTotalNotPageSize()
    {
        var f = new AccessFixture();

        var result = await new ListRolesUseCase(f.Roles).ExecuteAsync(
            new RolesQuery { Page = 0, PageSize = 1 },
            CancellationToken.None);

        Assert.Equal(2, result.Value.TotalCount);
        Assert.Single(result.Value.Items);
    }
}
