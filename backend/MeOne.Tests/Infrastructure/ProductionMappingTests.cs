using MeOne.Domain.Entities;
using MeOne.Domain.Enums;
using MeOne.Infrastructure.Data.Context;
using MeOne.Infrastructure.Mappings;
using Microsoft.EntityFrameworkCore;

namespace MeOne.Tests.Infrastructure;

public sealed class ProductionMappingTests
{
    private static MeOneDbContext Context() =>
        new(new DbContextOptionsBuilder<MeOneDbContext>()
            .UseMySql("Server=localhost;Database=meone;User=x;Password=x", new MySqlServerVersion(new Version(8, 0, 0)))
            .Options);

    [Fact]
    public void Tables_UseTheExpectedNames()
    {
        using var context = Context();

        foreach (var (type, table) in new[]
        {
            (typeof(ProductionLine), "production_lines"),
            (typeof(ProductionShift), "production_shifts"),
            (typeof(ProductionProduct), "production_products"),
            (typeof(LineLayout), "line_layouts"),
            (typeof(LayoutPost), "layout_posts"),
            (typeof(LayoutDecoration), "layout_decorations"),
            (typeof(ProductionRun), "production_runs"),
            (typeof(RunAllocation), "run_allocations"),
            (typeof(RunOutput), "run_outputs"),
        })
        {
            Assert.Equal(table, context.Model.FindEntityType(type)!.GetTableName());
        }
    }

    [Fact]
    public void Layouts_AreUniquePerLineProductAndShift()
    {
        using var context = Context();
        var entity = context.Model.FindEntityType(typeof(LineLayout))!;

        var index = entity.GetIndexes().Single(candidate => candidate.Properties.Count == 3);

        Assert.True(index.IsUnique);
        Assert.Equal(["fk_line", "fk_product", "fk_shift"], index.Properties.Select(property => property.GetColumnName()));
    }

    [Fact]
    public void TheOutputOfAnHour_IsUniquePerPost()
    {
        using var context = Context();
        var entity = context.Model.FindEntityType(typeof(RunOutput))!;

        var index = entity.GetIndexes().Single(candidate => candidate.IsUnique);

        Assert.Equal(["fk_run", "post_id", "hour_start"], index.Properties.Select(property => property.GetColumnName()));
    }

    [Fact]
    public void LayoutAndRunChildren_AreRemovedWithTheirParent()
    {
        using var context = Context();

        foreach (var (child, parent) in new[]
        {
            (typeof(LayoutPost), typeof(LineLayout)),
            (typeof(LayoutDecoration), typeof(LineLayout)),
            (typeof(RunAllocation), typeof(ProductionRun)),
            (typeof(RunOutput), typeof(ProductionRun)),
        })
        {
            Assert.Contains(
                context.Model.FindEntityType(child)!.GetForeignKeys(),
                key => key.PrincipalEntityType.ClrType == parent && key.DeleteBehavior == DeleteBehavior.Cascade);
        }
    }

    [Fact]
    public void TheSnapshotColumn_IsLongText()
    {
        using var context = Context();
        var property = context.Model.FindEntityType(typeof(ProductionRun))!.FindProperty(nameof(ProductionRun.Snapshot))!;

        Assert.Equal("layout_snapshot", property.GetColumnName());
        Assert.Equal("longtext", property.GetColumnType());
    }

    [Fact]
    public void TheSnapshotJson_RoundTripsPostsAndDecorations()
    {
        var snapshot = new RunSnapshot(
            [new RunPostSnapshot(5, "Montagem", PostKind.Machine, 30, 60, 220, 140, 90, true, 42.5m, "Soldar")],
            [new RunDecorationSnapshot(DecorationKind.Belt, 0, 0, 300, 40, 0, "Esteira")]);

        var json = RunSnapshotJson.Serialize(snapshot);
        var back = RunSnapshotJson.Deserialize(json);

        Assert.Contains("\"machine\"", json);
        Assert.Contains("\"belt\"", json);
        Assert.Equal(snapshot.Posts[0], back.Posts[0]);
        Assert.Equal(snapshot.Decorations[0], back.Decorations[0]);
    }

    [Fact]
    public void TheSnapshotComparer_TreatsEqualContentAsUnchanged()
    {
        var left = new RunSnapshot([new RunPostSnapshot(1, "A", PostKind.Standard, 0, 0, 220, 140, 0, false, null, null)], []);
        var right = new RunSnapshot([new RunPostSnapshot(1, "A", PostKind.Standard, 0, 0, 220, 140, 0, false, null, null)], []);
        var comparer = new RunSnapshotJson.Comparer();

        Assert.True(comparer.Equals(left, right));
        Assert.False(comparer.Equals(left, RunSnapshot.Empty));
    }
}
