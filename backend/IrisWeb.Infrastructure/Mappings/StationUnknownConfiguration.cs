using IrisWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IrisWeb.Infrastructure.Mappings;

public sealed class StationUnknownConfiguration : IEntityTypeConfiguration<StationUnknown>
{
    public void Configure(EntityTypeBuilder<StationUnknown> builder)
    {
        builder.ToTable("face_detector_station_unknown");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();

        builder.Property(x => x.Workstation)
            .HasColumnName("workstation")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Line)
            .HasColumnName("line")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.StartedAt)
            .HasColumnName("started_at")
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(x => x.EndedAt).HasColumnName("ended_at");
        builder.Property(x => x.DurationSeconds).HasColumnName("duration_seconds");

        builder.Property(x => x.CandidateRegistration)
            .HasColumnName("candidate_registration")
            .HasMaxLength(32);

        builder.Property(x => x.CandidateDistance).HasColumnName("candidate_distance");

        builder.Ignore(x => x.IsOpen);

        builder.HasIndex(x => new { x.Workstation, x.Line, x.StartedAt })
            .HasDatabaseName("idx_posto_linha_started");
    }
}
