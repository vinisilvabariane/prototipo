using MeOne.Domain.Entities;
using MeOne.Domain.Rules;

namespace MeOne.Tests.Domain;

public sealed class OrgChartTests
{
    private static OrgPosition Position(ulong id, ulong? parentId, string title = "Cargo") =>
        OrgPosition.Create(title, 1, parentId, null).WithId(id);

    private static List<OrgPosition> Chain(int length)
    {
        var positions = new List<OrgPosition>();
        ulong? parent = null;

        for (ulong id = 1; id <= (ulong)length; id++)
        {
            positions.Add(Position(id, parent));
            parent = id;
        }

        return positions;
    }

    [Fact]
    public void Create_TrimsTheTitle()
    {
        var position = OrgPosition.Create("  Diretor industrial ", 3, null, null);

        Assert.Equal("Diretor industrial", position.Title);
        Assert.Equal(3, position.DepartmentId);
    }

    [Fact]
    public void Update_ReportsWhetherAnythingChanged()
    {
        var position = OrgPosition.Create("Gerente", 1, null, null);

        Assert.False(position.Update(" Gerente ", 1));
        Assert.True(position.Update("Gerente de planta", 1));
        Assert.True(position.Update("Gerente de planta", 2));
    }

    [Fact]
    public void AssignHolder_AndMove_ReportChanges()
    {
        var position = OrgPosition.Create("Gerente", 1, null, null);

        Assert.True(position.AssignHolder(7));
        Assert.False(position.AssignHolder(7));
        Assert.True(position.AssignHolder(null));

        Assert.True(position.MoveTo(4));
        Assert.False(position.MoveTo(4));
        Assert.True(position.MoveTo(null));
    }

    [Fact]
    public void DepthOf_CountsLevelsFromTheRoot()
    {
        var positions = Chain(4);

        Assert.Equal(0, OrgChartHierarchy.DepthOf(positions, null));
        Assert.Equal(1, OrgChartHierarchy.DepthOf(positions, 1));
        Assert.Equal(4, OrgChartHierarchy.DepthOf(positions, 4));
    }

    [Fact]
    public void HeightOf_CountsTheLevelsOfTheSubtree()
    {
        var positions = Chain(4);
        positions.Add(Position(5, 2));

        Assert.Equal(4, OrgChartHierarchy.HeightOf(positions, 1));
        Assert.Equal(3, OrgChartHierarchy.HeightOf(positions, 2));
        Assert.Equal(1, OrgChartHierarchy.HeightOf(positions, 5));
    }

    [Fact]
    public void IsSelfOrBelow_FindsDescendantsAndTheNodeItself()
    {
        var positions = Chain(4);
        positions.Add(Position(5, null));

        Assert.True(OrgChartHierarchy.IsSelfOrBelow(positions, 2, 2));
        Assert.True(OrgChartHierarchy.IsSelfOrBelow(positions, 2, 4));
        Assert.False(OrgChartHierarchy.IsSelfOrBelow(positions, 2, 1));
        Assert.False(OrgChartHierarchy.IsSelfOrBelow(positions, 2, 5));
        Assert.False(OrgChartHierarchy.IsSelfOrBelow(positions, 2, null));
    }

    [Fact]
    public void Walks_SurviveACorruptCycle()
    {
        var a = Position(1, 2);
        var b = Position(2, 1);
        List<OrgPosition> positions = [a, b];

        Assert.True(OrgChartHierarchy.DepthOf(positions, 1) > 0);
        Assert.True(OrgChartHierarchy.HeightOf(positions, 1) >= 0);
        Assert.False(OrgChartHierarchy.IsSelfOrBelow(positions, 9, 1));
    }
}

internal static class OrgPositionTestExtensions
{
    public static OrgPosition WithId(this OrgPosition position, ulong id)
    {
        position.Id = id;
        return position;
    }
}
