using MeOne.Domain.Entities;
using MeOne.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace MeOne.Tests.Infrastructure;

public sealed class OrgChartMappingTests
{
    private static MeOneDbContext Context() =>
        new(new DbContextOptionsBuilder<MeOneDbContext>()
            .UseMySql("Server=localhost;Database=meone;User=x;Password=x", new MySqlServerVersion(new Version(8, 0, 0)))
            .Options);

    [Fact]
    public void OrgPositions_UseSnakeCaseColumns()
    {
        using var context = Context();
        var entity = context.Model.FindEntityType(typeof(OrgPosition))!;
        var columns = entity.GetProperties().ToDictionary(p => p.Name, p => p.GetColumnName());

        Assert.Equal("org_positions", entity.GetTableName());
        Assert.Equal("title", columns[nameof(OrgPosition.Title)]);
        Assert.Equal("fk_department", columns[nameof(OrgPosition.DepartmentId)]);
        Assert.Equal("fk_parent", columns[nameof(OrgPosition.ParentId)]);
        Assert.Equal("fk_holder", columns[nameof(OrgPosition.HolderId)]);
    }

    [Fact]
    public void AUserCanHoldOnlyOnePosition()
    {
        using var context = Context();
        var entity = context.Model.FindEntityType(typeof(OrgPosition))!;

        var index = entity.GetIndexes().Single(candidate => candidate.Properties.Single().Name == nameof(OrgPosition.HolderId));

        Assert.True(index.IsUnique);
    }
}
