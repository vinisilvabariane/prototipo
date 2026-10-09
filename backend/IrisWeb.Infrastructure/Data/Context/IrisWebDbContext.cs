using IrisWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IrisWeb.Infrastructure.Data.Context;

public sealed class IrisWebDbContext(DbContextOptions<IrisWebDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IrisWebDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
