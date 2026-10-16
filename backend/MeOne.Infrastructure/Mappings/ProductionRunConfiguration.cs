using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeOne.Infrastructure.Mappings;

public sealed class ProductionRunConfiguration : IEntityTypeConfiguration<ProductionRun>
{
    public void Configure(EntityTypeBuilder<ProductionRun> builder)
    {
        builder.ToTable("production_runs");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.LayoutId)
            .HasColumnName("layout_id")
            .IsRequired();

        builder.Property(x => x.LineId)
            .HasColumnName("fk_line")
            .IsRequired();

        builder.Property(x => x.ProductId)
            .HasColumnName("fk_product")
            .IsRequired();

        builder.Property(x => x.ShiftId)
            .HasColumnName("fk_shift")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Snapshot)
            .HasColumnName("layout_snapshot")
            .HasColumnType("longtext")
            .HasConversion(new RunSnapshotJson.Converter(), new RunSnapshotJson.Comparer())
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasConversion<UtcDateTimeConverter>()
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasColumnName("fk_created_by")
            .IsRequired();

        builder.Property(x => x.StartedAt)
            .HasColumnName("started_at")
            .HasConversion<UtcNullableDateTimeConverter>();

        builder.Property(x => x.StartedBy)
            .HasColumnName("fk_started_by");

        builder.Property(x => x.EndedAt)
            .HasColumnName("ended_at")
            .HasConversion<UtcNullableDateTimeConverter>();

        builder.Property(x => x.EndedBy)
            .HasColumnName("fk_ended_by");

        builder.Ignore(x => x.OpenAllocations);
        builder.Ignore(x => x.IsActive);

        builder.HasIndex(x => new { x.LineId, x.Status });
        builder.HasIndex(x => x.LayoutId);
        builder.HasIndex(x => x.CreatedAt);

        builder.HasOne<ProductionLine>()
            .WithMany()
            .HasForeignKey(x => x.LineId)
            .HasConstraintName("production_runs_fk_line")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ProductionProduct>()
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .HasConstraintName("production_runs_fk_product")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ProductionShift>()
            .WithMany()
            .HasForeignKey(x => x.ShiftId)
            .HasConstraintName("production_runs_fk_shift")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.CreatedBy)
            .HasConstraintName("production_runs_fk_created_by")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.StartedBy)
            .HasConstraintName("production_runs_fk_started_by")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.EndedBy)
            .HasConstraintName("production_runs_fk_ended_by")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Allocations)
            .WithOne()
            .HasForeignKey(x => x.RunId)
            .HasConstraintName("run_allocations_fk_run")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Outputs)
            .WithOne()
            .HasForeignKey(x => x.RunId)
            .HasConstraintName("run_outputs_fk_run")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Allocations).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.Outputs).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
