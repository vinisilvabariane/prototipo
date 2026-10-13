using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Rules;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeOne.Infrastructure.Mappings;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public const ulong MasterRoleId = 1;

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

        builder.HasData(new { Id = MasterRoleId, Name = AccessRoles.Master, Description = "Administração" });
    }
}
