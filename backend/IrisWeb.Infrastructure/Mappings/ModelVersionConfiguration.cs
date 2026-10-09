using IrisWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IrisWeb.Infrastructure.Persistence.Configurations;

public sealed class ModelVersionConfiguration : IEntityTypeConfiguration<ModelVersion>
{
    public void Configure(EntityTypeBuilder<ModelVersion> builder)
    {
        builder.ToTable("system_model_versions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.ModelId).HasColumnName("model_id");

        builder.Property(x => x.Version).HasColumnName("version").HasMaxLength(64).IsRequired();
        builder.HasIndex(x => new { x.ModelId, x.Version }).IsUnique().HasDatabaseName("uk_model_version");

        builder.Property(x => x.Runtime).HasColumnName("runtime").HasMaxLength(64).IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.ArtifactObjectKey).HasColumnName("artifact_object_key").HasMaxLength(500);
        builder.Property(x => x.ArtifactSize).HasColumnName("artifact_size");
        builder.Property(x => x.Sha256).HasColumnName("sha256").HasMaxLength(64);

        builder.Property(x => x.ManifestJson).HasColumnName("manifest");

        builder.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(200).IsRequired();
        builder.Property(x => x.ApprovedBy).HasColumnName("approved_by").HasMaxLength(200);
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at");
        builder.Property(x => x.ApprovedAtUtc).HasColumnName("approved_at");
    }
}
