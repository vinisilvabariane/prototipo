using MeOne.Application.Communication.Departments.Requests;
using MeOne.Application.Errors;
using MeOne.Application.UseCases.Departments;
using MeOne.Domain.Errors;
using MeOne.Tests.Fakes;

namespace MeOne.Tests.Application;

public sealed class DepartmentUseCaseTests
{
    [Fact]
    public async Task Create_StoresLowercase()
    {
        var f = new AccessFixture();

        var result = await new CreateDepartmentUseCase(f.Departments).ExecuteAsync(
            new CreateDepartmentRequest("  Engineering "),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("engineering", result.Value.Name);
    }

    [Fact]
    public async Task Create_SameNameDifferentCase_Fails()
    {
        var f = new AccessFixture();

        var result = await new CreateDepartmentUseCase(f.Departments).ExecuteAsync(
            new CreateDepartmentRequest("TI"),
            CancellationToken.None);

        Assert.Equal(DepartmentErrors.NameTaken("ti"), Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Delete_WithLinkedDeactivatedUser_Rejects()
    {
        var f = new AccessFixture();
        f.AddUser(12345).Deactivate(modifiedBy: 1);

        var result = await new DeleteDepartmentUseCase(f.Departments).ExecuteAsync(f.Ti.Id, CancellationToken.None);

        Assert.Equal(DepartmentErrors.InUse, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Delete_Unused_Removes()
    {
        var f = new AccessFixture();

        var result = await new DeleteDepartmentUseCase(f.Departments).ExecuteAsync(f.Ti.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(f.Departments.Stored);
    }

    [Fact]
    public async Task Update_Unknown_NotFound()
    {
        var f = new AccessFixture();

        var result = await new UpdateDepartmentUseCase(f.Departments).ExecuteAsync(
            999,
            new UpdateDepartmentRequest("rh"),
            CancellationToken.None);

        Assert.True(result.HasError<NotFoundError>());
    }

    [Fact]
    public async Task ListNames_ReturnsOnlyNames()
    {
        var f = new AccessFixture();
        await new CreateDepartmentUseCase(f.Departments).ExecuteAsync(new CreateDepartmentRequest("RH"), CancellationToken.None);

        var result = await new ListDepartmentNamesUseCase(f.Departments).ExecuteAsync(CancellationToken.None);

        Assert.Equal(["rh", "ti"], result.Value);
    }
}
