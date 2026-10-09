using IrisWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IrisWeb.Infrastructure.Mappings;

public sealed class EnrollmentConfirmationConfiguration : IEntityTypeConfiguration<EnrollmentConfirmation>
{
    public void Configure(EntityTypeBuilder<EnrollmentConfirmation> builder)
    {
        builder.ToTable("face_detector_enroll_confirmation");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();

        builder.Property(x => x.Registration)
            .HasColumnName("registration")
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.SimilarRegistration)
            .HasColumnName("similar_registration")
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.Distance)
            .HasColumnName("distance")
            .IsRequired();

        builder.Property(x => x.ConfirmedBy)
            .HasColumnName("confirmed_by")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.ConfirmedAt)
            .HasColumnName("confirmed_at")
            .ValueGeneratedNever()
            .IsRequired();

        builder.HasIndex(x => x.Registration).HasDatabaseName("idx_registration");
    }
}
