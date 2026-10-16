namespace MeOne.Infrastructure.Storage;

public sealed class SupplierStorageOptions
{
    public const string SectionName = "Supplier:Storage";

    public string Path { get; set; } = "uploads/suppliers";
}
