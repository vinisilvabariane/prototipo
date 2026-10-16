using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeOne.Infrastructure.Mappings;

public sealed class ProductionShiftConfiguration : IEntityTypeConfiguration<ProductionShift>
{
    public void Configure(EntityTypeBuilder<ProductionShift> builder)
    {
        builder.ToTable("production_shifts");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(ProductionErrors.MaxShiftNameLength)
            .IsRequired();

        builder.Property(x => x.StartTime)
            .HasColumnName("start_time")
            .IsRequired();

        builder.Property(x => x.EndTime)
            .HasColumnName("end_time")
            .IsRequired();

        builder.Property(x => x.Active)
            .HasColumnName("active")
            .IsRequired();

        builder.HasIndex(x => x.Name).IsUnique();
    }
}
