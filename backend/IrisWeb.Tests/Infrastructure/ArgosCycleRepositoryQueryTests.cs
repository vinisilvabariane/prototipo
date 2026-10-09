using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Enums;
using IrisWeb.Domain.Interfaces;
using IrisWeb.Infrastructure.Data.Context;
using IrisWeb.Infrastructure.Mappings;
using IrisWeb.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace IrisWeb.Tests.Infrastructure;

// Sem banco: ToQueryString() so gera o SQL, nao conecta. Cobre o que o AGENTS.md diz que o
// compilador nao pega: nome de coluna e se o filtro de turno traduz para SQL.
public sealed class ArgosCycleRepositoryQueryTests
{
    private static readonly DateTime From = new(2026, 10, 1);

    private static IrisWebDbContext Context() =>
        new(new DbContextOptionsBuilder<IrisWebDbContext>()
            .UseMySql("Server=localhost;Database=iris;User=x;Password=x", new MySqlServerVersion(new Version(8, 0, 0)))
            .Options);

    private static string Sql(ArgosCycleFilter filter)
    {
        using var context = Context();

        return ArgosCycleRepository.Apply(context.Set<ArgosCycle>(), filter).ToQueryString();
    }

    private static ArgosCycleFilter Filter(
        ArgosShiftFilter shift = ArgosShiftFilter.All,
        ArgosResultFilter result = ArgosResultFilter.All,
        string? station = null,
        string? setup = null) =>
        new(From, From.AddDays(1), shift, result, station, setup);

    [Fact]
    public void Mapping_UsesTheColumnNamesOfTheDesktopTable()
    {
        using var context = Context();
        var entity = context.Model.FindEntityType(typeof(ArgosCycle))!;

        Assert.Equal("route_verify_chronoanalysis", entity.GetTableName());

        var columns = entity.GetProperties().ToDictionary(p => p.Name, p => p.GetColumnName());

        Assert.Equal("cycle_time_s", columns[nameof(ArgosCycle.CycleSeconds)]);
        Assert.Equal("setup_name", columns[nameof(ArgosCycle.SetupName)]);
        Assert.Equal("fail_reason", columns[nameof(ArgosCycle.FailReason)]);
        Assert.Equal("failed_point", columns[nameof(ArgosCycle.FailedPoint)]);
        Assert.Equal("min_confidence", columns[nameof(ArgosCycle.MinConfidence)]);
        Assert.Equal(ArgosCycleConfiguration.RecordedAtColumn, columns[nameof(ArgosCycle.RecordedAt)]);
    }

    [Fact]
    public void Apply_AlwaysBoundsTheWindowInSql()
    {
        var sql = Sql(Filter());

        Assert.Contains("route_verify_chronoanalysis", sql);
        Assert.Contains($"`{ArgosCycleConfiguration.RecordedAtColumn}` >=", sql);
        Assert.Contains($"`{ArgosCycleConfiguration.RecordedAtColumn}` <", sql);
    }

    [Fact]
    public void Apply_ShiftTranslatesToATimeOfDayComparison()
    {
        Assert.Contains("AS time", Sql(Filter(shift: ArgosShiftFilter.Adm)));
        Assert.Contains(" OR ", Sql(Filter(shift: ArgosShiftFilter.T2)));
        Assert.DoesNotContain("AS time", Sql(Filter()));
    }

    [Fact]
    public void Apply_ResultStationAndSetupBecomeEqualityFilters()
    {
        var sql = Sql(Filter(result: ArgosResultFilter.Ok, station: "RV-01", setup: "CABO-A"));

        Assert.Contains("`status` = 1", sql);
        Assert.Contains("'RV-01'", sql);
        Assert.Contains("'CABO-A'", sql);
        Assert.Contains("`status` <> 1", Sql(Filter(result: ArgosResultFilter.Ng)));
    }
}
