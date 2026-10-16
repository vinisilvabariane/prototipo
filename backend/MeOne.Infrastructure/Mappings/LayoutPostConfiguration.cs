using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeOne.Infrastructure.Mappings;

public sealed class LayoutPostConfiguration : IEntityTypeConfiguration<LayoutPost>
{
    public void Configure(EntityTypeBuilder<LayoutPost> builder)
    {
        builder.ToTable("layout_posts");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.LayoutId)
            .HasColumnName("fk_layout")
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(ProductionErrors.MaxPostNameLength)
            .IsRequired();

        builder.Property(x => x.Kind)
            .HasColumnName("kind")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.X).HasColumnName("x").IsRequired();
        builder.Property(x => x.Y).HasColumnName("y").IsRequired();
        builder.Property(x => x.Width).HasColumnName("width").IsRequired();
        builder.Property(x => x.Height).HasColumnName("height").IsRequired();
        builder.Property(x => x.Rotation).HasColumnName("rotation").IsRequired();

        builder.Property(x => x.IsCritical)
            .HasColumnName("is_critical")
            .IsRequired();

        builder.Property(x => x.TargetPerHour)
            .HasColumnName("target_per_hour")
            .HasPrecision(10, 2);

        builder.Property(x => x.Operation)
            .HasColumnName("operation")
            .HasMaxLength(ProductionErrors.MaxOperationLength);

        builder.HasIndex(x => x.LayoutId);
    }
}
