using MeOne.Domain.Entities;
using MeOne.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MeOne.Infrastructure.Repositories;

public class DbContextRepository<TEntity, TId, TContext>(TContext context)
    : IRepository<TEntity, TId>
    where TEntity : Entity<TId>
    where TContext : DbContext
{
    protected readonly TContext Context = context;

    protected readonly DbSet<TEntity> DbSet = context.Set<TEntity>();

    public virtual async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken) =>
        await DbSet.FindAsync([id], cancellationToken);

    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken) =>
        await DbSet.AsNoTracking().ToListAsync(cancellationToken);

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken) =>
        await DbSet.AddAsync(entity, cancellationToken);

    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken) =>
        await DbSet.AddRangeAsync(entities, cancellationToken);

    public virtual void Update(TEntity entity) => DbSet.Update(entity);

    public virtual void Delete(TEntity entity) => DbSet.Remove(entity);

    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        await Context.SaveChangesAsync(cancellationToken);
}

public class Repository<TEntity, TId>(Data.Context.MeOneDbContext context)
    : DbContextRepository<TEntity, TId, Data.Context.MeOneDbContext>(context)
    where TEntity : Entity<TId>
{
}

public class Repository<TEntity>(Data.Context.MeOneDbContext context)
    : Repository<TEntity, Guid>(context), IRepository<TEntity>
    where TEntity : Entity
{
}
