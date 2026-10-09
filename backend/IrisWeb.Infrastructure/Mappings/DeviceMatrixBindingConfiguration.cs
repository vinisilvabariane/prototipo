using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IrisWeb.Infrastructure.Mappings;

public sealed class DeviceMatrixBindingConfiguration : IEntityTypeConfiguration<DeviceMatrixBinding>
{
    public void Configure(EntityTypeBuilder<DeviceMatrixBinding> builder)
    {
        builder.ToTable("face_detector_device_matrix");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("device_id")
            .HasMaxLength(MatrixErrors.MaxDeviceIdLength)
            .ValueGeneratedNever()
            .IsRequired();

        builder.Property(x => x.Line)
            .HasColumnName("sync_line")
            .HasMaxLength(MatrixErrors.MaxLineLength)
            .IsRequired();

        builder.Property(x => x.Product)
            .HasColumnName("sync_product")
            .HasMaxLength(MatrixErrors.MaxProductLength)
            .IsRequired();

        builder.Property(x => x.Shift)
            .HasColumnName("sync_shift")
            .HasMaxLength(MatrixErrors.MaxShiftLength)
            .IsRequired();

        builder.Property(x => x.StationId)
            .HasColumnName("sync_post_id")
            .IsRequired();

        builder.Property(x => x.StationName)
            .HasColumnName("sync_post_name")
            .HasMaxLength(MatrixErrors.MaxStationNameLength)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
    }
}
