using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeOne.Infrastructure.Mappings;

public sealed class WorkflowConfiguration : IEntityTypeConfiguration<Workflow>
{
    public const int EnumColumnLength = 30;

    public void Configure(EntityTypeBuilder<Workflow> builder)
    {
        builder.ToTable("workflows");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasMaxLength(WorkflowErrors.MaxTitleLength)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(WorkflowErrors.MaxDescriptionLength)
            .IsRequired();

        builder.Property(x => x.Priority)
            .HasColumnName("priority")
            .HasConversion<string>()
            .HasMaxLength(EnumColumnLength)
            .IsRequired();

        builder.Property(x => x.Category)
            .HasColumnName("category")
            .HasConversion<string>()
            .HasMaxLength(EnumColumnLength);

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(EnumColumnLength)
            .IsRequired();

        builder.Property(x => x.CreatorId)
            .HasColumnName("fk_creator")
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasConversion<UtcDateTimeConverter>()
            .IsRequired();

        builder.Property(x => x.CompletedAt)
            .HasColumnName("completed_at")
            .HasConversion<UtcNullableDateTimeConverter>();

        builder.Property(x => x.DueDate)
            .HasColumnName("due_date");

        builder.Property(x => x.Version)
            .HasColumnName("version")
            .IsConcurrencyToken()
            .IsRequired();

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.CreatorId);
        builder.HasIndex(x => x.CreatedAt);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.CreatorId)
            .HasConstraintName("workflows_fk_creator")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Assignees)
            .WithOne()
            .HasForeignKey(x => x.WorkflowId)
            .HasConstraintName("workflow_assignees_fk_workflow")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Steps)
            .WithOne()
            .HasForeignKey(x => x.WorkflowId)
            .HasConstraintName("workflow_steps_fk_workflow")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Attachments)
            .WithOne()
            .HasForeignKey(x => x.WorkflowId)
            .HasConstraintName("workflow_attachments_fk_workflow")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Messages)
            .WithOne()
            .HasForeignKey(x => x.WorkflowId)
            .HasConstraintName("workflow_messages_fk_workflow")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Assignees).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.Steps).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.Attachments).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.Messages).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(x => x.IsOpen);
        builder.Ignore(x => x.EveryoneApproved);
        builder.Ignore(x => x.HasRejectionAwaitingCreator);
    }
}
