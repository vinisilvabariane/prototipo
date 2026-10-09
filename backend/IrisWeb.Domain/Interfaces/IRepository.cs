using IrisWeb.Domain.Entities;

namespace IrisWeb.Domain.Interfaces;

public interface IRepository<TEntity, TId> where TEntity : Entity<TId>
{
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken);

    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken);

    Task AddAsync(TEntity entity, CancellationToken cancellationToken);

    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken);

    void Update(TEntity entity);

    void Delete(TEntity entity);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

public interface IRepository<TEntity> : IRepository<TEntity, Guid> where TEntity : Entity
{
}
