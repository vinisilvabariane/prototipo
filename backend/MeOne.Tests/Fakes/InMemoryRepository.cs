using MeOne.Domain.Entities;
using MeOne.Domain.Interfaces;

namespace MeOne.Tests.Fakes;

public class InMemoryRepository<TEntity, TId> : IRepository<TEntity, TId>
    where TEntity : Entity<TId>
{
    protected readonly List<TEntity> Items = [];

    private int _pending;

    private long _lastIdentity;

    public IReadOnlyList<TEntity> Stored => Items;

    public int SaveCount { get; private set; }

    public void Seed(params TEntity[] entities) => Items.AddRange(entities);

    public void Insert(params TEntity[] entities)
    {
        foreach (var entity in entities)
        {
            AssignIdentity(entity);
            Items.Add(entity);
        }
    }

    public virtual Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken) =>
        Task.FromResult(Items.FirstOrDefault(item => EqualityComparer<TId>.Default.Equals(item.Id, id)));

    public Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<TEntity>>(Items);

    public Task AddAsync(TEntity entity, CancellationToken cancellationToken)
    {
        AssignIdentity(entity);
        Items.Add(entity);
        _pending++;
        return Task.CompletedTask;
    }

    public Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
    {
        foreach (var entity in entities)
        {
            AssignIdentity(entity);
            Items.Add(entity);
            _pending++;
        }

        return Task.CompletedTask;
    }

    public void Update(TEntity entity) => _pending++;

    public void Delete(TEntity entity)
    {
        Items.Remove(entity);
        _pending++;
    }

    private void AssignIdentity(TEntity entity)
    {
        switch (entity)
        {
            case Entity<long> { Id: 0 } withLongId:
                withLongId.Id = ++_lastIdentity;
                break;
            case Entity<ulong> { Id: 0 } withULongId:
                withULongId.Id = (ulong)++_lastIdentity;
                break;
            case Entity<int> { Id: 0 } withIntId:
                withIntId.Id = (int)++_lastIdentity;
                break;
            case Entity<long> withLongId:
                _lastIdentity = Math.Max(_lastIdentity, withLongId.Id);
                break;
            case Entity<ulong> withULongId:
                _lastIdentity = Math.Max(_lastIdentity, (long)withULongId.Id);
                break;
            case Entity<int> withIntId:
                _lastIdentity = Math.Max(_lastIdentity, withIntId.Id);
                break;
        }
    }

    public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        SaveCount++;
        var affected = _pending;
        _pending = 0;
        return Task.FromResult(affected);
    }
}
