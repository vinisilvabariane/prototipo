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

        builder.Property(x => x.ScopeId)
            .HasColumnName("fk_scope");

        builder.HasIndex(x => new { x.UserId, x.RoleId, x.ScopeId }).IsUnique();

        builder.HasOne(x => x.Role)
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .HasConstraintName("user_roles_fk_role")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Scope)
            .WithMany()
            .HasForeignKey(x => x.ScopeId)
            .HasConstraintName("user_roles_fk_scope")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
