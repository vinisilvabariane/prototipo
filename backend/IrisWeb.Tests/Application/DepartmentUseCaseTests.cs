using IrisWeb.Application.Communication.Departments.Requests;
using IrisWeb.Application.Errors;
using IrisWeb.Application.UseCases.Departments;
using IrisWeb.Domain.Errors;
using IrisWeb.Tests.Fakes;

namespace IrisWeb.Tests.Application;

public sealed class DepartmentUseCaseTests
{
    [Fact]
    public async Task Create_GuardaEmMinusculas()
    {
        var f = new AccessFixture();

        var result = await new CreateDepartmentUseCase(f.Departments).ExecuteAsync(
            new CreateDepartmentRequest("  Engenharia "),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("engenharia", result.Value.Name);
    }

    [Fact]
    public async Task Create_MesmoNomeComOutraCaixa_Falha()
    {
        var f = new AccessFixture();

        var result = await new CreateDepartmentUseCase(f.Departments).ExecuteAsync(
            new CreateDepartmentRequest("TI"),
            CancellationToken.None);

        Assert.Equal(DepartmentErrors.NameTaken("ti"), Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Delete_ComUsuarioDesativadoVinculado_Recusa()
    {
        var f = new AccessFixture();
        f.AddUser(12345).Deactivate(modifiedBy: 1);

        var result = await new DeleteDepartmentUseCase(f.Departments).ExecuteAsync(f.Ti.Id, CancellationToken.None);

        Assert.Equal(DepartmentErrors.InUse, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Delete_Livre_Remove()
    {
        var f = new AccessFixture();

        var result = await new DeleteDepartmentUseCase(f.Departments).ExecuteAsync(f.Ti.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(f.Departments.Stored);
    }

    [Fact]
    public async Task Update_Inexistente_NotFound()
    {
        var f = new AccessFixture();

        var result = await new UpdateDepartmentUseCase(f.Departments).ExecuteAsync(
            999,
            new UpdateDepartmentRequest("rh"),
            CancellationToken.None);

        Assert.True(result.HasError<NotFoundError>());
    }

    [Fact]
    public async Task ListNames_DevolveSoOsNomes()
    {
        var f = new AccessFixture();
        await new CreateDepartmentUseCase(f.Departments).ExecuteAsync(new CreateDepartmentRequest("RH"), CancellationToken.None);

        var result = await new ListDepartmentNamesUseCase(f.Departments).ExecuteAsync(CancellationToken.None);

        Assert.Equal(["rh", "ti"], result.Value);
    }
}
