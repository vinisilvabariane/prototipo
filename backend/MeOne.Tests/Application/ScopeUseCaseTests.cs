using MeOne.Application.Communication.Scopes.Requests;
using MeOne.Application.Errors;
using MeOne.Application.UseCases.Scopes;
using MeOne.Domain.Enums;
using MeOne.Domain.Errors;
using MeOne.Tests.Fakes;

namespace MeOne.Tests.Application;

public sealed class ScopeUseCaseTests
{
    [Fact]
    public async Task Create_LineUnderArea_NormalizesCode()
    {
        var f = new AccessFixture();

        var result = await new CreateScopeUseCase(f.Scopes).ExecuteAsync(
            new CreateScopeRequest(ScopeType.Line, " line-a2 ", "Linha A2", f.AreaA.Id),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("LINE-A2", result.Value.Code);
        Assert.Equal(f.AreaA.Id, result.Value.ParentId);
    }

    [Fact]
    public async Task Create_LineUnderPlant_RejectsWrongParentType()
    {
        var f = new AccessFixture();

        var result = await new CreateScopeUseCase(f.Scopes).ExecuteAsync(
            new CreateScopeRequest(ScopeType.Line, "LINE-X", "Linha X", f.Plant.Id),
            CancellationToken.None);

        Assert.Equal(ScopeErrors.ParentMustBe("area"), Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Create_AreaWithoutParent_Rejects()
    {
        var f = new AccessFixture();

        var result = await new CreateScopeUseCase(f.Scopes).ExecuteAsync(
            new CreateScopeRequest(ScopeType.Area, "AREA-X", "Área X", null),
            CancellationToken.None);

        Assert.Equal(ScopeErrors.ParentRequired("plant"), Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Create_PlantWithParent_Rejects()
    {
        var f = new AccessFixture();

        var result = await new CreateScopeUseCase(f.Scopes).ExecuteAsync(
            new CreateScopeRequest(ScopeType.Plant, "PLT-2", "Planta 2", f.Plant.Id),
            CancellationToken.None);

        Assert.Equal(ScopeErrors.PlantHasNoParent, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Create_DuplicateCode_Rejects()
    {
        var f = new AccessFixture();

        var result = await new CreateScopeUseCase(f.Scopes).ExecuteAsync(
            new CreateScopeRequest(ScopeType.Plant, "plt-1", "Outra planta", null),
            CancellationToken.None);

        Assert.Equal(ScopeErrors.CodeTaken("PLT-1"), Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Delete_WithChildren_Rejects()
    {
        var f = new AccessFixture();

        var result = await new DeleteScopeUseCase(f.Scopes).ExecuteAsync(f.AreaA.Id, CancellationToken.None);

        Assert.Equal(ScopeErrors.HasChildren, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Delete_WithAssignments_Rejects()
    {
        var f = new AccessFixture();
        f.AddUser(12345).GrantRole(f.OperatorRole, f.AreaB);

        var result = await new DeleteScopeUseCase(f.Scopes).ExecuteAsync(f.AreaB.Id, CancellationToken.None);

        Assert.Equal(ScopeErrors.InUse, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Delete_Leaf_Removes()
    {
        var f = new AccessFixture();

        var result = await new DeleteScopeUseCase(f.Scopes).ExecuteAsync(f.StationA1.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.DoesNotContain(f.Scopes.Stored, scope => scope.Id == f.StationA1.Id);
    }

    [Fact]
    public async Task Update_Unknown_NotFound()
    {
        var f = new AccessFixture();

        var result = await new UpdateScopeUseCase(f.Scopes).ExecuteAsync(999, new UpdateScopeRequest("X", "X"), CancellationToken.None);

        Assert.True(result.HasError<NotFoundError>());
    }

    [Fact]
    public async Task Chain_IncludesSelfAndEveryAncestor()
    {
        var f = new AccessFixture();

        var chain = await f.Scopes.GetChainAsync(f.StationA1.Id, CancellationToken.None);

        Assert.Equal([f.StationA1.Id, f.LineA1.Id, f.AreaA.Id, f.Plant.Id], chain);
    }
}
