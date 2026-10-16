using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeOne.Infrastructure.Mappings;

public sealed class WorkflowAttachmentConfiguration : IEntityTypeConfiguration<WorkflowAttachment>
{
    public const int StoredNameLength = 100;

    public void Configure(EntityTypeBuilder<WorkflowAttachment> builder)
    {
        builder.ToTable("workflow_attachments");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.WorkflowId)
            .HasColumnName("fk_workflow")
            .IsRequired();

        builder.Property(x => x.OriginalName)
            .HasColumnName("original_name")
            .HasMaxLength(WorkflowErrors.MaxAttachmentNameLength)
            .IsRequired();

        builder.Property(x => x.StoredName)
            .HasColumnName("stored_name")
            .HasMaxLength(StoredNameLength)
            .IsRequired();

        builder.Property(x => x.ContentType)
            .HasColumnName("content_type")
            .HasMaxLength(WorkflowErrors.MaxContentTypeLength)
            .IsRequired();

        builder.Property(x => x.Size)
            .HasColumnName("size")
            .IsRequired();

        builder.Property(x => x.UploadedAt)
            .HasColumnName("uploaded_at")
            .HasConversion<UtcDateTimeConverter>()
            .IsRequired();

        builder.HasIndex(x => x.StoredName).IsUnique();
    }
}
