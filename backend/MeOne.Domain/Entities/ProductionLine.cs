namespace MeOne.Domain.Entities;

public sealed class ProductionLine : Entity<ulong>
{
    private ProductionLine() { }

    public string Name { get; private set; } = string.Empty;

    public bool Active { get; private set; } = true;

    public DateTime CreatedAt { get; private set; }

    public static ProductionLine Create(string name, DateTime now) => new()
    {
        Name = NormalizeName(name),
        CreatedAt = now,
    };

    public bool Update(string name, bool active)
    {
        var normalized = NormalizeName(name);

        if (Name == normalized && Active == active)
            return false;

        Name = normalized;
        Active = active;
        return true;
    }

    public static string NormalizeName(string name) => name.Trim();
}
