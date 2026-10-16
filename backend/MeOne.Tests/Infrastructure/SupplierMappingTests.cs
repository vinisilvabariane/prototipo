using MeOne.Domain.Entities;
using MeOne.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace MeOne.Tests.Infrastructure;

public sealed class SupplierMappingTests
{
    private static MeOneDbContext Context() =>
        new(new DbContextOptionsBuilder<MeOneDbContext>()
            .UseMySql("Server=localhost;Database=meone;User=x;Password=x", new MySqlServerVersion(new Version(8, 0, 0)))
            .Options);

    [Fact]
    public void Suppliers_UseSnakeCaseColumnsAndStoreEnumsAsText()
    {
        using var context = Context();
        var entity = context.Model.FindEntityType(typeof(Supplier))!;
        var columns = entity.GetProperties().ToDictionary(p => p.Name, p => p.GetColumnName());

        Assert.Equal("suppliers", entity.GetTableName());
        Assert.Equal("sap_code", columns[nameof(Supplier.SapCode)]);
        Assert.Equal("multi_branch", columns[nameof(Supplier.MultiBranch)]);
        Assert.Equal("fk_created_by", columns[nameof(Supplier.CreatedBy)]);
        Assert.DoesNotContain(nameof(Supplier.CurrentQualification), columns.Keys);
        Assert.Equal(typeof(string), entity.FindProperty(nameof(Supplier.Origin))!.GetProviderClrType());
    }

    [Fact]
    public void TheSapCode_IsUniquePerMultiBranch()
    {
        using var context = Context();
        var entity = context.Model.FindEntityType(typeof(Supplier))!;

        var index = entity.GetIndexes().Single(candidate => candidate.Properties.Select(p => p.Name).SequenceEqual([nameof(Supplier.SapCode), nameof(Supplier.MultiBranch)]));

        Assert.True(index.IsUnique);
    }

    [Fact]
    public void ChildTables_ReferenceTheSupplier()
    {
        using var context = Context();

        foreach (var (type, table) in new[]
        {
            (typeof(SupplierTool), "supplier_tools"),
            (typeof(SupplierMaterial), "supplier_materials"),
            (typeof(SupplierSelfAssessment), "supplier_self_assessments"),
            (typeof(SupplierAudit), "supplier_audits"),
            (typeof(SupplierDocument), "supplier_documents"),
        })
        {
            var entity = context.Model.FindEntityType(type)!;

            Assert.Equal(table, entity.GetTableName());
            Assert.Contains(entity.GetForeignKeys(), key => key.PrincipalEntityType.ClrType == typeof(Supplier) && key.DeleteBehavior == DeleteBehavior.Cascade);
        }
    }

    [Fact]
    public void Scores_UseTwoDecimals()
    {
        using var context = Context();

        Assert.Equal(2, context.Model.FindEntityType(typeof(SupplierAudit))!.FindProperty(nameof(SupplierAudit.Score))!.GetScale());
        Assert.Equal(2, context.Model.FindEntityType(typeof(SupplierSelfAssessment))!.FindProperty(nameof(SupplierSelfAssessment.Score))!.GetScale());
    }
}
