namespace MeOne.Domain.Entities;

public sealed class ProductionProduct : Entity<ulong>
{
    private ProductionProduct() { }

    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public bool Active { get; private set; } = true;

    public static ProductionProduct Create(string code, string name) => new()
    {
        Code = NormalizeCode(code),
        Name = name.Trim(),
    };

    public bool Update(string code, string name, bool active)
    {
        var next = (Code: NormalizeCode(code), Name: name.Trim(), Active: active);

        if ((Code, Name, Active) == next)
            return false;

        (Code, Name, Active) = next;
        return true;
    }

    public static string NormalizeCode(string code) => code.Trim().ToUpperInvariant();
}
