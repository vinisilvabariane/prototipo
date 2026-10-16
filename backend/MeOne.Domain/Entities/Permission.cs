namespace MeOne.Domain.Entities;

public sealed class Permission : Entity<string>
{
    private Permission() { }

    public string Description { get; private set; } = string.Empty;

    public bool GlobalOnly { get; private set; }
}
