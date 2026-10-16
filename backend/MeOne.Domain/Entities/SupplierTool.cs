namespace MeOne.Domain.Entities;

public sealed class SupplierTool : Entity<ulong>
{
    private SupplierTool() { }

    public ulong SupplierId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string? AssetTag { get; private set; }

    public bool Active { get; private set; } = true;

    internal static SupplierTool Create(string name, string? assetTag) => new()
    {
        Name = name.Trim(),
        AssetTag = Clean(assetTag),
    };

    public bool Update(string name, string? assetTag)
    {
        var next = (Name: name.Trim(), AssetTag: Clean(assetTag));

        if ((Name, AssetTag) == next)
            return false;

        (Name, AssetTag) = next;
        return true;
    }

    internal void Remove() => Active = false;

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
