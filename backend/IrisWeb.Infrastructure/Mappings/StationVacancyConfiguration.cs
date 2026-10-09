using IrisWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IrisWeb.Infrastructure.Mappings;

public sealed class StationVacancyConfiguration : IEntityTypeConfiguration<StationVacancy>
{
    public void Configure(EntityTypeBuilder<StationVacancy> builder)
    {
        builder.ToTable("face_detector_station_vacancy");

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

        builder.Ignore(x => x.IsOpen);

        builder.HasIndex(x => new { x.Workstation, x.Line, x.StartedAt })
            .HasDatabaseName("idx_posto_linha_started");
    }
}
