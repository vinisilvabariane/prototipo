using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Rules;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeOne.Infrastructure.Mappings;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public const ulong MasterRoleId = RoleCatalog.AdministratorId;

    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(RoleErrors.MaxNameLength)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(RoleErrors.MaxDescriptionLength)
            .IsRequired();

        builder.HasIndex(x => x.Name).IsUnique();

        builder.HasMany(x => x.Permissions)
            .WithOne()
            .HasForeignKey(x => x.RoleId)
            .HasConstraintName("role_permissions_fk_role")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Permissions).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(x => x.IsAdministrator);
        builder.Ignore(x => x.PermissionCodes);

        builder.HasData(RoleCatalog.All.Select(role => new { role.Id, role.Name, role.Description }));
    }
}
