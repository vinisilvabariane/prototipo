using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeOne.Infrastructure.Mappings;

public sealed class OrgPositionConfiguration : IEntityTypeConfiguration<OrgPosition>
{
    public void Configure(EntityTypeBuilder<OrgPosition> builder)
    {
        builder.ToTable("org_positions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Title)
            .HasColumnName("title")
            .HasMaxLength(OrgChartErrors.MaxTitleLength)
            .IsRequired();

        builder.Property(x => x.DepartmentId)
            .HasColumnName("fk_department")
            .IsRequired();

        builder.Property(x => x.ParentId)
            .HasColumnName("fk_parent");

        builder.Property(x => x.HolderId)
            .HasColumnName("fk_holder");

        builder.HasIndex(x => x.HolderId).IsUnique();
        builder.HasIndex(x => x.ParentId);

        builder.HasOne<Department>()
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .HasConstraintName("org_positions_fk_department")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<OrgPosition>()
            .WithMany()
            .HasForeignKey(x => x.ParentId)
            .HasConstraintName("org_positions_fk_parent")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.HolderId)
            .HasConstraintName("org_positions_fk_holder")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
