using MeOne.Application.Communication.OrgChart.Requests;
using MeOne.Application.Errors;
using MeOne.Application.UseCases.OrgChart;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Rules;
using MeOne.Tests.Fakes;

namespace MeOne.Tests.Application;

public sealed class OrgChartUseCaseTests
{
    private sealed class Setup
    {
        public Setup()
        {
            Access = new AccessFixture();
            Positions = new FakeOrgPositionRepository();
            Ana = Access.AddUser(2001, "ana@example.com", "Ana Diretora");
            Bruno = Access.AddUser(2002, "bruno@example.com", "Bruno Gerente");
            Carla = Access.AddUser(2003, "carla@example.com", "Carla Analista");
        }

        public AccessFixture Access { get; }

        public FakeOrgPositionRepository Positions { get; }

        public User Ana { get; }

        public User Bruno { get; }

        public User Carla { get; }

        public int DepartmentId => Access.Ti.Id;

        public CreateOrgPositionUseCase Create => new(Positions, Access.Departments, Access.Users);

        public OrgPosition Add(string title, ulong? parentId = null, User? holder = null)
        {
            var position = OrgPosition.Create(title, DepartmentId, parentId, holder?.Id);
            Positions.Insert(position);
            return position;
        }
    }

    [Fact]
    public async Task Create_RootWithHolder_StoresAndReturnsTheNames()
    {
        var s = new Setup();

        var result = await s.Create.ExecuteAsync(
            new CreateOrgPositionRequest("  Diretor industrial ", s.DepartmentId, null, s.Ana.Id),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Diretor industrial", result.Value.Title);
        Assert.Equal("ti", result.Value.Department);
        Assert.Equal("Ana Diretora", result.Value.Holder?.Name);
        Assert.Single(s.Positions.Stored);
    }

    [Fact]
    public async Task Create_AllowsAVacantPosition()
    {
        var s = new Setup();

        var result = await s.Create.ExecuteAsync(
            new CreateOrgPositionRequest("Coordenador", s.DepartmentId, null, null),
            CancellationToken.None);

        Assert.Null(result.Value.Holder);
    }

    [Fact]
    public async Task Create_UnknownDepartment_NotFound()
    {
        var s = new Setup();

        var result = await s.Create.ExecuteAsync(new CreateOrgPositionRequest("Cargo", 999, null, null), CancellationToken.None);

        Assert.True(result.HasError<NotFoundError>());
    }

    [Fact]
    public async Task Create_UnknownParent_NotFound()
    {
        var s = new Setup();

        var result = await s.Create.ExecuteAsync(
            new CreateOrgPositionRequest("Cargo", s.DepartmentId, 999, null),
            CancellationToken.None);

        Assert.Equal(OrgChartErrors.ParentNotFound, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Create_HolderAlreadyInAnotherPosition_Fails()
    {
        var s = new Setup();
        s.Add("Diretor", holder: s.Ana);

        var result = await s.Create.ExecuteAsync(
            new CreateOrgPositionRequest("Gerente", s.DepartmentId, null, s.Ana.Id),
            CancellationToken.None);

        Assert.Equal(OrgChartErrors.HolderTaken("Diretor"), Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Create_DeactivatedHolder_NotFound()
    {
        var s = new Setup();
        s.Ana.Deactivate(modifiedBy: 1);

        var result = await s.Create.ExecuteAsync(
            new CreateOrgPositionRequest("Gerente", s.DepartmentId, null, s.Ana.Id),
            CancellationToken.None);

        Assert.True(result.HasError<NotFoundError>());
    }

    [Fact]
    public async Task Create_BeyondTheMaximumDepth_Fails()
    {
        var s = new Setup();
        OrgPosition? last = null;

        for (var level = 0; level < OrgChartHierarchy.MaxDepth; level++)
            last = s.Add($"Nível {level}", last?.Id);

        var result = await s.Create.ExecuteAsync(
            new CreateOrgPositionRequest("Fundo", s.DepartmentId, last!.Id, null),
            CancellationToken.None);

        Assert.Equal(OrgChartErrors.TooDeep, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Update_ChangesTitleAndDepartment()
    {
        var s = new Setup();
        var position = s.Add("Gerente");
        var rh = s.Access.Departments.Stored.First();

        var result = await new UpdateOrgPositionUseCase(s.Positions, s.Access.Departments).ExecuteAsync(
            position.Id,
            new UpdateOrgPositionRequest("Gerente de planta", rh.Id),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Gerente de planta", position.Title);
    }

    [Fact]
    public async Task Update_UnknownPosition_NotFound()
    {
        var s = new Setup();

        var result = await new UpdateOrgPositionUseCase(s.Positions, s.Access.Departments).ExecuteAsync(
            404,
            new UpdateOrgPositionRequest("x", s.DepartmentId),
            CancellationToken.None);

        Assert.True(result.HasError<NotFoundError>());
    }

    [Fact]
    public async Task AssignHolder_ThenVacate()
    {
        var s = new Setup();
        var position = s.Add("Gerente");
        var assign = new AssignOrgPositionHolderUseCase(s.Positions, s.Access.Users);

        var assigned = await assign.ExecuteAsync(position.Id, new AssignOrgPositionHolderRequest(s.Bruno.Id), CancellationToken.None);
        Assert.True(assigned.IsSuccess);
        Assert.Equal(s.Bruno.Id, position.HolderId);

        var vacated = await assign.ExecuteAsync(position.Id, new AssignOrgPositionHolderRequest(null), CancellationToken.None);
        Assert.True(vacated.IsSuccess);
        Assert.Null(position.HolderId);
    }

    [Fact]
    public async Task AssignHolder_AlreadyHoldingAnotherPosition_Fails()
    {
        var s = new Setup();
        s.Add("Diretor", holder: s.Ana);
        var other = s.Add("Gerente");

        var result = await new AssignOrgPositionHolderUseCase(s.Positions, s.Access.Users)
            .ExecuteAsync(other.Id, new AssignOrgPositionHolderRequest(s.Ana.Id), CancellationToken.None);

        Assert.Equal(OrgChartErrors.HolderTaken("Diretor"), Assert.Single(result.Errors).Message);
        Assert.Null(other.HolderId);
    }

    [Fact]
    public async Task AssignHolder_TheSameHolderAgain_IsANoOp()
    {
        var s = new Setup();
        var position = s.Add("Diretor", holder: s.Ana);

        var result = await new AssignOrgPositionHolderUseCase(s.Positions, s.Access.Users)
            .ExecuteAsync(position.Id, new AssignOrgPositionHolderRequest(s.Ana.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task AssignHolder_UnknownUser_NotFound()
    {
        var s = new Setup();
        var position = s.Add("Gerente");

        var result = await new AssignOrgPositionHolderUseCase(s.Positions, s.Access.Users)
            .ExecuteAsync(position.Id, new AssignOrgPositionHolderRequest(999), CancellationToken.None);

        Assert.True(result.HasError<NotFoundError>());
    }

    [Fact]
    public async Task Move_ReparentsAPositionWithItsSubtree()
    {
        var s = new Setup();
        var root = s.Add("Diretor");
        var other = s.Add("Gerente A", root.Id);
        var moving = s.Add("Gerente B", root.Id);
        var child = s.Add("Analista", moving.Id);

        var result = await new MoveOrgPositionUseCase(s.Positions)
            .ExecuteAsync(moving.Id, new MoveOrgPositionRequest(other.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(other.Id, moving.ParentId);
        Assert.Equal(moving.Id, child.ParentId);
    }

    [Fact]
    public async Task Move_ToTheRoot_ClearsTheParent()
    {
        var s = new Setup();
        var root = s.Add("Diretor");
        var child = s.Add("Gerente", root.Id);

        var result = await new MoveOrgPositionUseCase(s.Positions)
            .ExecuteAsync(child.Id, new MoveOrgPositionRequest(null), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(child.ParentId);
    }

    [Fact]
    public async Task Move_UnderItselfOrADescendant_Fails()
    {
        var s = new Setup();
        var root = s.Add("Diretor");
        var child = s.Add("Gerente", root.Id);
        var grandchild = s.Add("Analista", child.Id);
        var move = new MoveOrgPositionUseCase(s.Positions);

        var underSelf = await move.ExecuteAsync(root.Id, new MoveOrgPositionRequest(root.Id), CancellationToken.None);
        var underDescendant = await move.ExecuteAsync(root.Id, new MoveOrgPositionRequest(grandchild.Id), CancellationToken.None);

        Assert.Equal(OrgChartErrors.MoveUnderItself, Assert.Single(underSelf.Errors).Message);
        Assert.Equal(OrgChartErrors.MoveUnderItself, Assert.Single(underDescendant.Errors).Message);
        Assert.Null(root.ParentId);
    }

    [Fact]
    public async Task Move_UnknownParent_NotFound()
    {
        var s = new Setup();
        var position = s.Add("Gerente");

        var result = await new MoveOrgPositionUseCase(s.Positions)
            .ExecuteAsync(position.Id, new MoveOrgPositionRequest(999), CancellationToken.None);

        Assert.Equal(OrgChartErrors.ParentNotFound, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Move_ThatMakesTheTreeTooDeep_Fails()
    {
        var s = new Setup();
        OrgPosition? last = null;

        for (var level = 0; level < OrgChartHierarchy.MaxDepth; level++)
            last = s.Add($"Nível {level}", last?.Id);

        var branch = s.Add("Ramo");
        s.Add("Folha", branch.Id);

        var result = await new MoveOrgPositionUseCase(s.Positions)
            .ExecuteAsync(branch.Id, new MoveOrgPositionRequest(last!.Id), CancellationToken.None);

        Assert.Equal(OrgChartErrors.TooDeep, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Delete_LeafPosition_Removes()
    {
        var s = new Setup();
        var position = s.Add("Gerente");

        var result = await new DeleteOrgPositionUseCase(s.Positions).ExecuteAsync(position.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(s.Positions.Stored);
    }

    [Fact]
    public async Task Delete_PositionWithSubordinates_Fails()
    {
        var s = new Setup();
        var root = s.Add("Diretor");
        s.Add("Gerente", root.Id);

        var result = await new DeleteOrgPositionUseCase(s.Positions).ExecuteAsync(root.Id, CancellationToken.None);

        Assert.Equal(OrgChartErrors.HasSubordinates, Assert.Single(result.Errors).Message);
        Assert.Equal(2, s.Positions.Stored.Count);
    }

    [Fact]
    public async Task Delete_Unknown_NotFound()
    {
        var s = new Setup();

        var result = await new DeleteOrgPositionUseCase(s.Positions).ExecuteAsync(404, CancellationToken.None);

        Assert.True(result.HasError<NotFoundError>());
    }

    [Fact]
    public async Task List_ReturnsPositionsWithHoldersDepartmentsAndTheManageFlag()
    {
        var s = new Setup();
        var root = s.Add("Diretor", holder: s.Ana);
        s.Add("Analista", root.Id);
        s.Bruno.Deactivate(modifiedBy: 1);
        s.Add("Gerente", root.Id, s.Bruno);

        var asManager = await new ListOrgChartUseCase(s.Positions, s.Access.Departments, s.Access.Users, FakeCurrentActor.Administrator())
            .ExecuteAsync(CancellationToken.None);
        var asReader = await new ListOrgChartUseCase(s.Positions, s.Access.Departments, s.Access.Users, FakeCurrentActor.For(s.Carla))
            .ExecuteAsync(CancellationToken.None);

        Assert.Equal(["Analista", "Diretor", "Gerente"], asManager.Value.Positions.Select(position => position.Title));
        Assert.True(asManager.Value.CanManage);
        Assert.False(asReader.Value.CanManage);

        var manager = asManager.Value.Positions.Single(position => position.Title == "Gerente");
        Assert.Equal("Bruno Gerente", manager.Holder?.Name);
        Assert.False(manager.Holder?.Active);
        Assert.Equal("ti", manager.Department);
    }

    [Fact]
    public async Task List_ManageFlagFollowsThePermission()
    {
        var s = new Setup();

        var withPermission = await new ListOrgChartUseCase(
                s.Positions,
                s.Access.Departments,
                s.Access.Users,
                FakeCurrentActor.WithPermissions(s.Carla.Id, Permissions.OrgChartManage))
            .ExecuteAsync(CancellationToken.None);

        Assert.True(withPermission.Value.CanManage);
    }

    [Fact]
    public async Task AssignableUsers_ExcludeThoseWhoAlreadyHoldAPosition()
    {
        var s = new Setup();
        s.Add("Diretor", holder: s.Ana);

        var result = await new ListOrgAssignableUsersUseCase(s.Positions, s.Access.Users).ExecuteAsync(CancellationToken.None);

        Assert.DoesNotContain(result.Value, user => user.Id == s.Ana.Id);
        Assert.Contains(result.Value, user => user.Id == s.Bruno.Id && user.Department == "ti");
    }
}
