using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeOne.Infrastructure.Mappings;

public sealed class SupplierAuditConfiguration : IEntityTypeConfiguration<SupplierAudit>
{
    public void Configure(EntityTypeBuilder<SupplierAudit> builder)
    {
        builder.ToTable("supplier_audits");

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

        builder.Property(x => x.AuditedOn)
            .HasColumnName("audited_on")
            .IsRequired();

        builder.Property(x => x.ValidityMonths)
            .HasColumnName("validity_months")
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

        builder.Property(x => x.ChecklistCode)
            .HasColumnName("checklist_code")
            .HasMaxLength(SupplierErrors.MaxDocumentCodeLength);

        builder.Property(x => x.ChecklistRevision)
            .HasColumnName("checklist_revision");

        builder.Property(x => x.Active)
            .HasColumnName("active")
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.ResponsibleId)
            .HasConstraintName("supplier_audits_fk_responsible")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
