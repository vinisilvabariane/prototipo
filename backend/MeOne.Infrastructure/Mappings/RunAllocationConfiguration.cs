using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeOne.Infrastructure.Mappings;

public sealed class RunAllocationConfiguration : IEntityTypeConfiguration<RunAllocation>
{
    public void Configure(EntityTypeBuilder<RunAllocation> builder)
    {
        builder.ToTable("run_allocations");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.RunId)
            .HasColumnName("fk_run")
            .IsRequired();

        builder.Property(x => x.PostId)
            .HasColumnName("post_id")
            .IsRequired();

        builder.Property(x => x.UserId)
            .HasColumnName("fk_user")
            .IsRequired();

        builder.Property(x => x.AllocatedAt)
            .HasColumnName("allocated_at")
            .HasConversion<UtcDateTimeConverter>()
            .IsRequired();

        builder.Property(x => x.ReleasedAt)
            .HasColumnName("released_at")
            .HasConversion<UtcNullableDateTimeConverter>();

        builder.Property(x => x.AllocatedBy)
            .HasColumnName("fk_allocated_by")
            .IsRequired();

        builder.Ignore(x => x.IsOpen);

        builder.HasIndex(x => new { x.RunId, x.PostId });
        builder.HasIndex(x => new { x.UserId, x.ReleasedAt });

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("run_allocations_fk_user")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.AllocatedBy)
            .HasConstraintName("run_allocations_fk_allocated_by")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
