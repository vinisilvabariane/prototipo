using MeOne.Domain.Enums;

namespace MeOne.Domain.Entities;

public sealed class SupplierMaterial : Entity<ulong>
{
    private SupplierMaterial() { }

    public ulong SupplierId { get; private set; }

    public string Sku { get; private set; } = string.Empty;

    public string? BusinessUnit { get; private set; }

    public string? MaterialType { get; private set; }

    public SupplierCriticality? Criticality { get; private set; }

    public bool? HasSapSpecification { get; private set; }

    public bool Active { get; private set; } = true;

    internal static SupplierMaterial Create(
        string sku,
        string? businessUnit,
        string? materialType,
        SupplierCriticality? criticality,
        bool? hasSapSpecification)
    {
        var material = new SupplierMaterial();
        material.Update(sku, businessUnit, materialType, criticality, hasSapSpecification);
        return material;
    }

    public bool Update(
        string sku,
        string? businessUnit,
        string? materialType,
        SupplierCriticality? criticality,
        bool? hasSapSpecification)
    {
        var next = (
            Sku: sku.Trim().ToUpperInvariant(),
            BusinessUnit: Clean(businessUnit),
            MaterialType: Clean(materialType),
            Criticality: criticality,
            HasSapSpecification: hasSapSpecification);

        if ((Sku, BusinessUnit, MaterialType, Criticality, HasSapSpecification) == next)
            return false;

        (Sku, BusinessUnit, MaterialType, Criticality, HasSapSpecification) = next;
        return true;
    }

    internal void Remove() => Active = false;

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
