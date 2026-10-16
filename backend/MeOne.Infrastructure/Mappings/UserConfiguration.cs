using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeOne.Infrastructure.Mappings;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Registration)
            .HasColumnName("registration")
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(UserErrors.MaxNameLength)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasColumnName("email")
            .HasMaxLength(UserErrors.MaxEmailLength);

        builder.Property(x => x.PasswordHash)
            .HasColumnName("password")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Active)
            .HasColumnName("active")
            .IsRequired();

        builder.Property(x => x.ModifiedAt)
            .HasColumnName("modified_at")
            .IsRequired();

        builder.Property(x => x.ModifiedBy)
            .HasColumnName("modified_by")
            .IsRequired();

        builder.Property(x => x.DepartmentId)
            .HasColumnName("fk_department")
            .IsRequired();

        builder.Property(x => x.SecurityStamp)
            .HasColumnName("security_stamp")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.FailedLoginCount)
            .HasColumnName("failed_login_count")
            .IsRequired();

        builder.Property(x => x.LockedUntil)
            .HasColumnName("locked_until");

        builder.HasIndex(x => x.Registration).IsUnique();
        builder.HasIndex(x => x.Email).IsUnique();

        builder.HasQueryFilter(x => x.Active);

        builder.HasOne(x => x.Department)
            .WithMany()
            .HasForeignKey(x => x.DepartmentId)
            .HasConstraintName("users_fk_department")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Roles)
            .WithOne()
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("user_roles_fk_user")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Applications)
            .WithOne()
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("user_applications_fk_user")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Roles).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(x => x.Applications).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(x => x.RoleNames);
        builder.Ignore(x => x.ApplicationNames);
        builder.Ignore(x => x.IsAdministrator);
    }
}
