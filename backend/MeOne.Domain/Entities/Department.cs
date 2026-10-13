namespace MeOne.Domain.Entities;

public sealed class Department : Entity<int>
{
    private Department() { }

    public string Name { get; private set; } = string.Empty;

    public static Department Create(string name) => new() { Name = Normalize(name) };

    public bool Rename(string name)
    {
        var normalized = Normalize(name);

        if (Name == normalized)
            return false;

        Name = normalized;
        return true;
    }

    public static string Normalize(string name) => name.Trim().ToLowerInvariant();
}
