using IrisWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IrisWeb.Infrastructure.Mappings;

public sealed class StationPresenceConfiguration : IEntityTypeConfiguration<StationPresence>
{
    public void Configure(EntityTypeBuilder<StationPresence> builder)
    {
        builder.ToTable("face_detector_station_presence");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();

        builder.Property(x => x.Registration)
            .HasColumnName("registration")
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.Workstation)
            .HasColumnName("workstation")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Line)
            .HasColumnName("line")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.EnteredAt)
            .HasColumnName("entered_at")
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(x => x.LeftAt).HasColumnName("left_at");
        builder.Property(x => x.DurationSeconds).HasColumnName("duration_seconds");

        builder.Property(x => x.CollaboratorName)
            .HasColumnName("collaborator_name")
            .HasMaxLength(StationPresence.MaxNameLength);

        builder.Property(x => x.TrainingLevel)
            .HasColumnName("training_level")
            .HasMaxLength(StationPresence.MaxLevelLength);

        builder.Property(x => x.AllocatedRegistration)
            .HasColumnName("allocated_registration")
            .HasMaxLength(StationPresence.MaxRegistrationLength);

        builder.Ignore(x => x.IsOpen);

        builder.HasIndex(x => x.Registration).HasDatabaseName("idx_registration");
        builder.HasIndex(x => new { x.Workstation, x.Line, x.EnteredAt })
            .HasDatabaseName("idx_posto_linha_entered");
    }
}
