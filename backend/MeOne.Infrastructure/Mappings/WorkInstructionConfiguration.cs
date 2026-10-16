using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeOne.Infrastructure.Mappings;

public sealed class WorkInstructionConfiguration : IEntityTypeConfiguration<WorkInstruction>
{
    public const int StoredNameLength = 100;

    public void Configure(EntityTypeBuilder<WorkInstruction> builder)
    {
        builder.ToTable("work_instructions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(WorkInstructionErrors.MaxCodeLength)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasMaxLength(WorkInstructionErrors.MaxTitleLength)
            .IsRequired();

        builder.Property(x => x.DocumentNumber)
            .HasColumnName("document_number")
            .HasMaxLength(WorkInstructionErrors.MaxDocumentNumberLength)
            .IsRequired();

        builder.Property(x => x.Product)
            .HasColumnName("product")
            .HasMaxLength(WorkInstructionErrors.MaxProductLength)
            .IsRequired();

        builder.Property(x => x.PreparedBy)
            .HasColumnName("prepared_by")
            .HasMaxLength(WorkInstructionErrors.MaxPersonLength)
            .IsRequired();

        builder.Property(x => x.ApprovedBy)
            .HasColumnName("approved_by")
            .HasMaxLength(WorkInstructionErrors.MaxPersonLength)
            .IsRequired();

        builder.Property(x => x.RequiredPpe)
            .HasColumnName("required_ppe")
            .HasMaxLength(WorkInstructionErrors.MaxPpeLength)
            .IsRequired();

        builder.Property(x => x.ProcessSpecifications)
            .HasColumnName("process_specifications")
            .HasMaxLength(WorkInstructionErrors.MaxSpecificationsLength)
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasColumnName("notes")
            .HasMaxLength(WorkInstructionErrors.MaxNotesLength);

        builder.Property(x => x.PdfStoredName)
            .HasColumnName("pdf_stored_name")
            .HasMaxLength(StoredNameLength)
            .IsRequired();

        builder.Property(x => x.PdfSize)
            .HasColumnName("pdf_size")
            .IsRequired();

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

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.Title);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.CreatedBy)
            .HasConstraintName("work_instructions_fk_created_by")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UpdatedBy)
            .HasConstraintName("work_instructions_fk_updated_by")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Steps)
            .WithOne()
            .HasForeignKey(x => x.WorkInstructionId)
            .HasConstraintName("work_instruction_steps_fk_instruction")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Steps).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
