using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeOne.Infrastructure.Mappings;

public sealed class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("suppliers");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.SapCode)
            .HasColumnName("sap_code")
            .HasMaxLength(SupplierErrors.MaxSapCodeLength)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(SupplierErrors.MaxNameLength)
            .IsRequired();

        builder.Property(x => x.Branch)
            .HasColumnName("branch")
            .HasMaxLength(SupplierErrors.MaxBranchLength)
            .IsRequired();

        builder.Property(x => x.MultiBranch)
            .HasColumnName("multi_branch")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Origin)
            .HasColumnName("origin")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.TaxId)
            .HasColumnName("tax_id")
            .HasMaxLength(SupplierErrors.MaxTaxIdLength);

        builder.Property(x => x.CorporateName)
            .HasColumnName("corporate_name")
            .HasMaxLength(SupplierErrors.MaxCorporateNameLength);

        builder.Property(x => x.Criticality)
            .HasColumnName("criticality")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.ZipCode)
            .HasColumnName("zip_code")
            .HasMaxLength(SupplierErrors.MaxZipCodeLength)
            .IsRequired();

        builder.Property(x => x.District)
            .HasColumnName("district")
            .HasMaxLength(SupplierErrors.MaxDistrictLength)
            .IsRequired();

        builder.Property(x => x.Street)
            .HasColumnName("street")
            .HasMaxLength(SupplierErrors.MaxStreetLength);

        builder.Property(x => x.Number)
            .HasColumnName("number")
            .HasMaxLength(SupplierErrors.MaxNumberLength);

        builder.Property(x => x.ContactName)
            .HasColumnName("contact_name")
            .HasMaxLength(SupplierErrors.MaxContactLength);

        builder.Property(x => x.Phone)
            .HasColumnName("phone")
            .HasMaxLength(SupplierErrors.MaxPhoneLength);

        builder.Property(x => x.Email)
            .HasColumnName("email")
            .HasMaxLength(SupplierErrors.MaxEmailLength);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasConversion<UtcDateTimeConverter>()
            .IsRequired();

        builder.Property(x => x.CreatedBy)
            .HasColumnName("fk_created_by")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasConversion<UtcDateTimeConverter>()
            .IsRequired();

        builder.Property(x => x.UpdatedBy)
            .HasColumnName("fk_updated_by")
            .IsRequired();

        builder.Ignore(x => x.CurrentQualification);
        builder.Ignore(x => x.Tools);
        builder.Ignore(x => x.Materials);
        builder.Ignore(x => x.SelfAssessments);
        builder.Ignore(x => x.Audits);

        builder.HasIndex(x => new { x.SapCode, x.MultiBranch }).IsUnique();
        builder.HasIndex(x => x.Name);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.CreatedBy)
            .HasConstraintName("suppliers_fk_created_by")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UpdatedBy)
            .HasConstraintName("suppliers_fk_updated_by")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany<SupplierTool>("_tools")
            .WithOne()
            .HasForeignKey(x => x.SupplierId)
            .HasConstraintName("supplier_tools_fk_supplier")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<SupplierMaterial>("_materials")
            .WithOne()
            .HasForeignKey(x => x.SupplierId)
            .HasConstraintName("supplier_materials_fk_supplier")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<SupplierSelfAssessment>("_selfAssessments")
            .WithOne()
            .HasForeignKey(x => x.SupplierId)
            .HasConstraintName("supplier_self_assessments_fk_supplier")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany<SupplierAudit>("_audits")
            .WithOne()
            .HasForeignKey(x => x.SupplierId)
            .HasConstraintName("supplier_audits_fk_supplier")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Documents)
            .WithOne()
            .HasForeignKey(x => x.SupplierId)
            .HasConstraintName("supplier_documents_fk_supplier")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Documents).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
