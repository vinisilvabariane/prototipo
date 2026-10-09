using IrisWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IrisWeb.Infrastructure.Persistence.Configurations;

public sealed class AiModelConfiguration : IEntityTypeConfiguration<AiModel>
{
    public void Configure(EntityTypeBuilder<AiModel> builder)
    {
        builder.ToTable("system_models");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Key).HasColumnName("model_key").HasMaxLength(100).IsRequired();
        builder.HasIndex(x => x.Key).IsUnique().HasDatabaseName("uk_model_key");

        builder.Property(x => x.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasColumnName("description").HasMaxLength(2000);

        builder.Property(x => x.TaskType)
            .HasColumnName("task_type")
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Platform)
            .HasColumnName("platform")
            .HasConversion<string>()
            .HasMaxLength(16)
            .IsRequired();

        builder.Property(x => x.ActiveVersionId)
            .HasColumnName("active_version_id")
            .HasMaxLength(36);

        builder.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(200).IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasColumnName("created_at");

        builder.HasMany(x => x.Versions)
            .WithOne(x => x.Model)
            .HasForeignKey(x => x.ModelId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
