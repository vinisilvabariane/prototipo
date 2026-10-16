using MeOne.Domain.Entities;
using MeOne.Domain.Rules;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeOne.Infrastructure.Mappings;

public sealed class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("role_permissions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.RoleId)
            .HasColumnName("fk_role")
            .IsRequired();

        builder.Property(x => x.PermissionCode)
            .HasColumnName("fk_permission")
            .HasMaxLength(PermissionConfiguration.MaxCodeLength)
            .IsRequired();

        builder.HasIndex(x => new { x.RoleId, x.PermissionCode }).IsUnique();

        builder.HasOne<Permission>()
            .WithMany()
            .HasForeignKey(x => x.PermissionCode)
            .HasConstraintName("role_permissions_fk_permission")
            .OnDelete(DeleteBehavior.Cascade);

        ulong id = 0;
        builder.HasData(RoleCatalog.All.SelectMany(role => role.Permissions.Select(code => new
        {
            Id = ++id,
            RoleId = role.Id,
            PermissionCode = code,
        })).ToList());
    }
}
