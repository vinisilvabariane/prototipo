using MeOne.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeOne.Infrastructure.Mappings;

public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_roles");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UserId)
            .HasColumnName("fk_user")
            .IsRequired();

        builder.Property(x => x.RoleId)
            .HasColumnName("fk_role")
            .IsRequired();

        builder.HasIndex(x => new { x.UserId, x.RoleId }).IsUnique();

        builder.HasOne(x => x.Role)
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .HasConstraintName("user_roles_fk_role")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
