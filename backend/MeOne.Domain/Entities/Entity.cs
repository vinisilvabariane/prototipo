namespace MeOne.Domain.Entities;

public abstract class Entity<TId>
{
    public TId Id { get; set; } = default!;
}

public abstract class Entity : Entity<Guid>
{
}
