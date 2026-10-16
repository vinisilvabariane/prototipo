using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeOne.Infrastructure.Mappings;

public sealed class RunOutputConfiguration : IEntityTypeConfiguration<RunOutput>
{
    public void Configure(EntityTypeBuilder<RunOutput> builder)
    {
        builder.ToTable("run_outputs");

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

        builder.Property(x => x.HourStart)
            .HasColumnName("hour_start")
            .HasConversion<UtcDateTimeConverter>()
            .IsRequired();

        builder.Property(x => x.Quantity)
            .HasColumnName("quantity")
            .IsRequired();

        builder.Property(x => x.UserId)
            .HasColumnName("fk_user");

        builder.Property(x => x.RecordedBy)
            .HasColumnName("fk_recorded_by")
            .IsRequired();

        builder.Property(x => x.RecordedAt)
            .HasColumnName("recorded_at")
            .HasConversion<UtcDateTimeConverter>()
            .IsRequired();

        builder.HasIndex(x => new { x.RunId, x.PostId, x.HourStart }).IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("run_outputs_fk_user")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.RecordedBy)
            .HasConstraintName("run_outputs_fk_recorded_by")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
