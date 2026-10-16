using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeOne.Infrastructure.Mappings;

public sealed class SupplierToolConfiguration : IEntityTypeConfiguration<SupplierTool>
{
    public void Configure(EntityTypeBuilder<SupplierTool> builder)
    {
        builder.ToTable("supplier_tools");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.SupplierId)
            .HasColumnName("fk_supplier")
            .IsRequired();

        builder.HasIndex(x => x.SupplierId);

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(SupplierErrors.MaxToolNameLength)
            .IsRequired();

        builder.Property(x => x.AssetTag)
            .HasColumnName("asset_tag")
            .HasMaxLength(SupplierErrors.MaxAssetTagLength);

        builder.Property(x => x.Active)
            .HasColumnName("active")
            .IsRequired();
    }
}
