using MeOne.Domain.Entities;
using MeOne.Domain.Rules;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeOne.Infrastructure.Mappings;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public const int MaxCodeLength = 50;

    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permissions");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("code")
            .HasMaxLength(MaxCodeLength)
            .ValueGeneratedNever();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.GlobalOnly)
            .HasColumnName("global_only")
            .IsRequired();

        builder.HasData(Permissions.All.Select(permission => new
        {
            Id = permission.Code,
            permission.Description,
            permission.GlobalOnly,
        }));
    }
}
