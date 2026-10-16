using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeOne.Infrastructure.Mappings;

public sealed class ProductionProductConfiguration : IEntityTypeConfiguration<ProductionProduct>
{
    public void Configure(EntityTypeBuilder<ProductionProduct> builder)
    {
        builder.ToTable("production_products");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(ProductionErrors.MaxProductCodeLength)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(ProductionErrors.MaxProductNameLength)
            .IsRequired();

        builder.Property(x => x.Active)
            .HasColumnName("active")
            .IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();
    }
}
