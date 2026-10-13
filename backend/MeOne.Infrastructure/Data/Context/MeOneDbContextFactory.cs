using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MeOne.Infrastructure.Data.Context;

public sealed class MeOneDbContextFactory : IDesignTimeDbContextFactory<MeOneDbContext>
{
    public const string LocalConnectionString =
        "Server=localhost;Port=3306;Database=meone;User=meone;Password=meone;";

    public MeOneDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__MeOne");

        if (string.IsNullOrWhiteSpace(connectionString))
            connectionString = LocalConnectionString;

        var options = new DbContextOptionsBuilder<MeOneDbContext>()
            .UseMySql(connectionString, MeOneDbContext.ServerVersion)
            .Options;

        return new MeOneDbContext(options);
    }
}
