using MeOne.Infrastructure.Data.Context;
using MeOne.Infrastructure.Mappings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace MeOne.Tests.Infrastructure;

public sealed class MigrationTests
{
    private static MeOneDbContext Context() => new MeOneDbContextFactory().CreateDbContext([]);

    [Fact]
    public void Mapping_HasNoChangesWithoutMigration()
    {
        using var context = Context();

        Assert.False(
            context.Database.HasPendingModelChanges(),
            "The mapping changed and there is no migration for it. Run: dotnet ef migrations add <Name> " +
            "--project MeOne.Infrastructure --startup-project MeOne.API");
    }

    [Fact]
    public void Migrations_CreateTablesAndMasterRole()
    {
        using var context = Context();

        var sql = context.GetService<IMigrator>().GenerateScript();

        foreach (var table in new[] { "users", "roles", "user_roles", "departments", "applications", "user_applications", "refresh_tokens" })
            Assert.Contains($"CREATE TABLE `{table}`", sql);

        Assert.Contains($"VALUES ({RoleConfiguration.MasterRoleId}, 'Administração', 'master')", sql);
    }

    [Fact]
    public void Factory_WithoutEnvironmentVariable_PointsToLocalCompose()
    {
        var previous = Environment.GetEnvironmentVariable("ConnectionStrings__MeOne");
        Environment.SetEnvironmentVariable("ConnectionStrings__MeOne", null);

        try
        {
            using var context = Context();

            var actual = new MySqlConnector.MySqlConnectionStringBuilder(context.Database.GetConnectionString()!);

            Assert.Equal("localhost", actual.Server);
            Assert.Equal(3306u, actual.Port);
            Assert.Equal("meone", actual.Database);
            Assert.Equal("meone", actual.UserID);
        }
        finally
        {
            Environment.SetEnvironmentVariable("ConnectionStrings__MeOne", previous);
        }
    }
}
