using Microsoft.EntityFrameworkCore;

namespace MeOne.Infrastructure.Data.Context;

public sealed class MeOneDbContext(DbContextOptions<MeOneDbContext> options) : DbContext(options)
{
    public static readonly MySqlServerVersion ServerVersion = new(new Version(8, 0, 0));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MeOneDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
