using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeOne.Infrastructure.Mappings;

public sealed class WorkflowStepConfiguration : IEntityTypeConfiguration<WorkflowStep>
{
    public void Configure(EntityTypeBuilder<WorkflowStep> builder)
    {
        builder.ToTable("workflow_steps");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.WorkflowId)
            .HasColumnName("fk_workflow")
            .IsRequired();

        builder.Property(x => x.UserId)
            .HasColumnName("fk_user")
            .IsRequired();

        builder.Property(x => x.Action)
            .HasColumnName("action")
            .HasConversion<string>()
            .HasMaxLength(WorkflowConfiguration.EnumColumnLength)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(WorkflowErrors.MaxStepDescriptionLength)
            .IsRequired();

        builder.Property(x => x.Justification)
            .HasColumnName("justification")
            .HasMaxLength(WorkflowErrors.MaxJustificationLength);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasConversion<UtcDateTimeConverter>()
            .IsRequired();

        builder.Property(x => x.Review)
            .HasColumnName("review")
            .HasConversion<string>()
            .HasMaxLength(WorkflowConfiguration.EnumColumnLength)
            .IsRequired();

        builder.HasIndex(x => new { x.WorkflowId, x.UserId });

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("workflow_steps_fk_user")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
