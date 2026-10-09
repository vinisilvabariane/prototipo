namespace IrisWeb.Domain.Entities;

/// <summary>
/// O nome é sempre guardado em minúsculas: a busca por nome compara exato, e o Access Control
/// antigo dependia de alguém lembrar de cadastrar em minúsculas.
/// </summary>
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
