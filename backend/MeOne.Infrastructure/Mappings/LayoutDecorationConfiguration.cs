using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeOne.Infrastructure.Mappings;

public sealed class LayoutDecorationConfiguration : IEntityTypeConfiguration<LayoutDecoration>
{
    public void Configure(EntityTypeBuilder<LayoutDecoration> builder)
    {
        builder.ToTable("layout_decorations");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.LayoutId)
            .HasColumnName("fk_layout")
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

        builder.Property(x => x.Label)
            .HasColumnName("label")
            .HasMaxLength(ProductionErrors.MaxLabelLength);

        builder.Property(x => x.Locked)
            .HasColumnName("locked")
            .IsRequired();

        builder.HasIndex(x => x.LayoutId);
    }
}
