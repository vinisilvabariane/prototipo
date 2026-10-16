using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeOne.Infrastructure.Mappings;

public sealed class SupplierSelfAssessmentConfiguration : IEntityTypeConfiguration<SupplierSelfAssessment>
{
    public void Configure(EntityTypeBuilder<SupplierSelfAssessment> builder)
    {
        builder.ToTable("supplier_self_assessments");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.SupplierId)
            .HasColumnName("fk_supplier")
            .IsRequired();

        builder.HasIndex(x => x.SupplierId);

        builder.Property(x => x.ResponsibleId)
            .HasColumnName("fk_responsible")
            .IsRequired();

        builder.Property(x => x.AssessedOn)
            .HasColumnName("assessed_on")
            .IsRequired();

        builder.Property(x => x.ExpiresOn)
            .HasColumnName("expires_on")
            .IsRequired();

        builder.Property(x => x.Score)
            .HasColumnName("score")
            .HasPrecision(5, 2);

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.EditableLink)
            .HasColumnName("editable_link")
            .HasMaxLength(SupplierErrors.MaxLinkLength);

        builder.Property(x => x.DocumentCode)
            .HasColumnName("document_code")
            .HasMaxLength(SupplierErrors.MaxDocumentCodeLength);

        builder.Property(x => x.Revision)
            .HasColumnName("revision");

        builder.Property(x => x.NeedsOnSiteAudit)
            .HasColumnName("needs_on_site_audit");

        builder.Property(x => x.Active)
            .HasColumnName("active")
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.ResponsibleId)
            .HasConstraintName("supplier_self_assessments_fk_responsible")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
