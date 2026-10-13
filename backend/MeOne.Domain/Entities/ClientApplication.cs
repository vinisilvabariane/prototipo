namespace MeOne.Domain.Entities;

public sealed class ClientApplication : Entity<ulong>
{
    private ClientApplication() { }

    public string Name { get; private set; } = string.Empty;

    public bool Active { get; private set; } = true;

    public static ClientApplication Create(string name) => new() { Name = name.Trim() };
}
