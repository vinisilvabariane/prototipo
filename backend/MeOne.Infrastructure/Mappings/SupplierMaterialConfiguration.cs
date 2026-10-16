using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeOne.Infrastructure.Mappings;

public sealed class SupplierMaterialConfiguration : IEntityTypeConfiguration<SupplierMaterial>
{
    public void Configure(EntityTypeBuilder<SupplierMaterial> builder)
    {
        builder.ToTable("supplier_materials");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.SupplierId)
            .HasColumnName("fk_supplier")
            .IsRequired();

        builder.HasIndex(x => x.SupplierId);

        builder.Property(x => x.Sku)
            .HasColumnName("sku")
            .HasMaxLength(SupplierErrors.MaxSkuLength)
            .IsRequired();

        builder.Property(x => x.BusinessUnit)
            .HasColumnName("business_unit")
            .HasMaxLength(SupplierErrors.MaxBusinessUnitLength);

        builder.Property(x => x.MaterialType)
            .HasColumnName("material_type")
            .HasMaxLength(SupplierErrors.MaxMaterialTypeLength);

        builder.Property(x => x.Criticality)
            .HasColumnName("criticality")
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.HasSapSpecification)
            .HasColumnName("has_sap_specification");

        builder.Property(x => x.Active)
            .HasColumnName("active")
            .IsRequired();
    }
}
