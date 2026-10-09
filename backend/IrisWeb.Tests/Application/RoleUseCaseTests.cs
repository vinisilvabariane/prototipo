using IrisWeb.Application.Communication.Roles.Requests;
using IrisWeb.Application.Errors;
using IrisWeb.Application.UseCases.Roles;
using IrisWeb.Application.UseCases.Users;
using IrisWeb.Domain.Errors;
using IrisWeb.Tests.Fakes;

namespace IrisWeb.Tests.Application;

public sealed class RoleUseCaseTests
{
    [Fact]
    public async Task Create_NomeRepetidoIgnorandoCaixa_Falha()
    {
        var f = new AccessFixture();

        var result = await new CreateRoleUseCase(f.Roles).ExecuteAsync(
            new CreateRoleRequest(" Operador ", "Outra"),
            CancellationToken.None);

        Assert.Equal(RoleErrors.NameTaken("Operador"), Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Create_DevolveARoleComId()
    {
        var f = new AccessFixture();

        var result = await new CreateRoleUseCase(f.Roles).ExecuteAsync(
            new CreateRoleRequest("qualidade", "Inspeção"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(0UL, result.Value.Id);
    }

    [Fact]
    public async Task Update_RenomearAMaster_Recusa()
    {
        var f = new AccessFixture();

        var result = await new UpdateRoleUseCase(f.Roles).ExecuteAsync(
            f.MasterRole.Id,
            new UpdateRoleRequest("admin", "Administração"),
            CancellationToken.None);

        Assert.Equal(RoleErrors.MasterRoleProtected, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Update_SoADescricaoDaMaster_Pode()
    {
        var f = new AccessFixture();

        var result = await new UpdateRoleUseCase(f.Roles).ExecuteAsync(
            f.MasterRole.Id,
            new UpdateRoleRequest("master", "Governança"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Governança", f.MasterRole.Description);
    }

    [Fact]
    public async Task Update_SemMudanca_ESucessoSemGravar()
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
    public async Task Delete_RoleEmUso_InclusivePorUsuarioDesativado_Recusa()
    {
        var f = new AccessFixture();
        f.AddUser(12345, roles: f.OperatorRole).Deactivate(modifiedBy: 1);

        var result = await new DeleteRoleUseCase(f.Roles).ExecuteAsync(f.OperatorRole.Id, CancellationToken.None);

        Assert.Equal(RoleErrors.InUse, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Delete_Master_Recusa()
    {
        var f = new AccessFixture();

        var result = await new DeleteRoleUseCase(f.Roles).ExecuteAsync(f.MasterRole.Id, CancellationToken.None);

        Assert.Equal(RoleErrors.MasterRoleProtected, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Delete_Inexistente_NotFound()
    {
        var f = new AccessFixture();

        var result = await new DeleteRoleUseCase(f.Roles).ExecuteAsync(999, CancellationToken.None);

        Assert.True(result.HasError<NotFoundError>());
    }

    [Fact]
    public async Task Assign_DaARoleERepetirFalha()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);
        var useCase = new AssignRoleUseCase(f.Users, f.Roles);

        var primeira = await useCase.ExecuteAsync(new AssignRoleRequest(user.Id, f.OperatorRole.Id), CancellationToken.None);
        var segunda = await useCase.ExecuteAsync(new AssignRoleRequest(user.Id, f.OperatorRole.Id), CancellationToken.None);

        Assert.True(primeira.IsSuccess);
        Assert.Equal(["operador"], user.RoleNames);
        Assert.Equal(RoleErrors.AlreadyAssigned, Assert.Single(segunda.Errors).Message);
    }

    [Fact]
    public async Task Assign_RoleOuUsuarioInexistente_NotFound()
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
    public async Task Revoke_TiraARole()
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
    public async Task Revoke_MasterTirandoAMasterDeSiMesmo_Recusa()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345, roles: f.MasterRole);

        var result = await new RevokeRoleUseCase(f.Users, FakeCurrentActor.Master(userId: user.Id))
            .ExecuteAsync(new RevokeRoleRequest(user.Id, f.MasterRole.Id), CancellationToken.None);

        Assert.Equal(RoleErrors.CannotRevokeOwnMaster, Assert.Single(result.Errors).Message);
        Assert.True(user.HasRole(f.MasterRole.Id));
    }

    [Fact]
    public async Task Revoke_RoleQueOUsuarioNaoTem_Falha()
    {
        var f = new AccessFixture();
        var user = f.AddUser(12345);

        var result = await new RevokeRoleUseCase(f.Users, FakeCurrentActor.Master())
            .ExecuteAsync(new RevokeRoleRequest(user.Id, f.OperatorRole.Id), CancellationToken.None);

        Assert.Equal(RoleErrors.NotAssigned, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task List_TotalEOTotalDaTabelaENaoODaPagina()
    {
        var f = new AccessFixture();

        var result = await new ListRolesUseCase(f.Roles).ExecuteAsync(
            new RolesQuery { Page = 0, PageSize = 1 },
            CancellationToken.None);

        Assert.Equal(2, result.Value.TotalCount);
        Assert.Single(result.Value.Items);
    }
}
