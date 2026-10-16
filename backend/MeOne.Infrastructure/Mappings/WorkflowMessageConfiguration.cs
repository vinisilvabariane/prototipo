using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeOne.Infrastructure.Mappings;

public sealed class WorkflowMessageConfiguration : IEntityTypeConfiguration<WorkflowMessage>
{
    public void Configure(EntityTypeBuilder<WorkflowMessage> builder)
    {
        builder.ToTable("workflow_messages");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.WorkflowId)
            .HasColumnName("fk_workflow")
            .IsRequired();

        builder.Property(x => x.StepId)
            .HasColumnName("fk_step")
            .IsRequired();

        builder.Property(x => x.UserId)
            .HasColumnName("fk_user")
            .IsRequired();

        builder.Property(x => x.Text)
            .HasColumnName("text")
            .HasMaxLength(WorkflowErrors.MaxJustificationLength)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasConversion<UtcDateTimeConverter>()
            .IsRequired();

        builder.HasIndex(x => x.StepId);

        builder.HasOne<WorkflowStep>()
            .WithMany()
            .HasForeignKey(x => x.StepId)
            .HasConstraintName("workflow_messages_fk_step")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("workflow_messages_fk_user")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
