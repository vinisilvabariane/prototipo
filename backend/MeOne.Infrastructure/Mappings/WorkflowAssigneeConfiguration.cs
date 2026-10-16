using MeOne.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeOne.Infrastructure.Mappings;

public sealed class WorkflowAssigneeConfiguration : IEntityTypeConfiguration<WorkflowAssignee>
{
    public void Configure(EntityTypeBuilder<WorkflowAssignee> builder)
    {
        builder.ToTable("workflow_assignees");

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

        builder.Property(x => x.AssignedAt)
            .HasColumnName("assigned_at")
            .HasConversion<UtcDateTimeConverter>()
            .IsRequired();

        builder.HasIndex(x => new { x.WorkflowId, x.UserId }).IsUnique();
        builder.HasIndex(x => x.UserId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("workflow_assignees_fk_user")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
