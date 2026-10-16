using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeOne.Infrastructure.Mappings;

public sealed class ProductionLineConfiguration : IEntityTypeConfiguration<ProductionLine>
{
    public void Configure(EntityTypeBuilder<ProductionLine> builder)
    {
        builder.ToTable("production_lines");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(ProductionErrors.MaxLineNameLength)
            .IsRequired();

        builder.Property(x => x.Active)
            .HasColumnName("active")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasConversion<UtcDateTimeConverter>()
            .IsRequired();

        builder.HasIndex(x => x.Name).IsUnique();
    }
}
