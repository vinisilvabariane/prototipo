using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeOne.Infrastructure.Mappings;

public sealed class LineLayoutConfiguration : IEntityTypeConfiguration<LineLayout>
{
    public void Configure(EntityTypeBuilder<LineLayout> builder)
    {
        builder.ToTable("line_layouts");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.LineId)
            .HasColumnName("fk_line")
            .IsRequired();

        builder.Property(x => x.ProductId)
            .HasColumnName("fk_product")
            .IsRequired();

        builder.Property(x => x.ShiftId)
            .HasColumnName("fk_shift")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasConversion<UtcDateTimeConverter>()
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasConversion<UtcDateTimeConverter>()
            .IsRequired();

        builder.Property(x => x.UpdatedBy)
            .HasColumnName("fk_updated_by")
            .IsRequired();

        builder.HasIndex(x => new { x.LineId, x.ProductId, x.ShiftId }).IsUnique();

        builder.HasOne<ProductionLine>()
            .WithMany()
            .HasForeignKey(x => x.LineId)
            .HasConstraintName("line_layouts_fk_line")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ProductionProduct>()
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .HasConstraintName("line_layouts_fk_product")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ProductionShift>()
            .WithMany()
            .HasForeignKey(x => x.ShiftId)
            .HasConstraintName("line_layouts_fk_shift")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UpdatedBy)
            .HasConstraintName("line_layouts_fk_updated_by")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Posts)
            .WithOne()
            .HasForeignKey(x => x.LayoutId)
            .HasConstraintName("layout_posts_fk_layout")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Decorations)
            .WithOne()
            .HasForeignKey(x => x.LayoutId)
            .HasConstraintName("layout_decorations_fk_layout")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Posts).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.Decorations).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
