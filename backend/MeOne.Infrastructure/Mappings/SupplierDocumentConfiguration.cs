using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeOne.Infrastructure.Mappings;

public sealed class SupplierDocumentConfiguration : IEntityTypeConfiguration<SupplierDocument>
{
    public const int StoredNameLength = 100;

    public void Configure(EntityTypeBuilder<SupplierDocument> builder)
    {
        builder.ToTable("supplier_documents");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.SupplierId)
            .HasColumnName("fk_supplier")
            .IsRequired();

        builder.HasIndex(x => x.SupplierId);

        builder.Property(x => x.Kind)
            .HasColumnName("kind")
            .HasConversion<string>()
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(x => x.OwnerId)
            .HasColumnName("owner_id");

        builder.Property(x => x.OriginalName)
            .HasColumnName("original_name")
            .HasMaxLength(SupplierErrors.MaxFileNameLength)
            .IsRequired();

        builder.Property(x => x.StoredName)
            .HasColumnName("stored_name")
            .HasMaxLength(StoredNameLength)
            .IsRequired();

        builder.Property(x => x.ContentType)
            .HasColumnName("content_type")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Size)
            .HasColumnName("size")
            .IsRequired();

        builder.Property(x => x.Version)
            .HasColumnName("version")
            .HasMaxLength(SupplierErrors.MaxVersionLength);

        builder.Property(x => x.ExpiresOn)
            .HasColumnName("expires_on");

        builder.Property(x => x.UploadedAt)
            .HasColumnName("uploaded_at")
            .HasConversion<UtcDateTimeConverter>()
            .IsRequired();

        builder.Property(x => x.UploadedBy)
            .HasColumnName("fk_uploaded_by")
            .IsRequired();

        builder.HasIndex(x => new { x.SupplierId, x.Kind, x.OwnerId });

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UploadedBy)
            .HasConstraintName("supplier_documents_fk_uploaded_by")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
