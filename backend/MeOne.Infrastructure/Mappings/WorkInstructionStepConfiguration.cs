using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeOne.Infrastructure.Mappings;

public sealed class WorkInstructionStepConfiguration : IEntityTypeConfiguration<WorkInstructionStep>
{
    public void Configure(EntityTypeBuilder<WorkInstructionStep> builder)
    {
        builder.ToTable("work_instruction_steps");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.WorkInstructionId)
            .HasColumnName("fk_instruction")
            .IsRequired();

        builder.Property(x => x.Position)
            .HasColumnName("position")
            .IsRequired();

        builder.Property(x => x.Text)
            .HasColumnName("text")
            .HasMaxLength(WorkInstructionErrors.MaxStepTextLength)
            .IsRequired();

        builder.Property(x => x.ImageStoredName)
            .HasColumnName("image_stored_name")
            .HasMaxLength(WorkInstructionConfiguration.StoredNameLength);

        builder.Property(x => x.ImageContentType)
            .HasColumnName("image_content_type")
            .HasMaxLength(50);

        builder.Ignore(x => x.HasImage);

        builder.HasIndex(x => new { x.WorkInstructionId, x.Position });
    }
}
