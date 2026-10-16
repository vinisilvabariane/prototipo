using MeOne.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeOne.Infrastructure.Mappings;

public sealed class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("password_reset_tokens");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UserId)
            .HasColumnName("fk_user")
            .IsRequired();

        builder.Property(x => x.CodeHash)
            .HasColumnName("code_hash")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .HasColumnName("expires")
            .IsRequired();

        builder.Property(x => x.UsedAt)
            .HasColumnName("used_at");

        builder.Property(x => x.FailedAttempts)
            .HasColumnName("failed_attempts")
            .IsRequired();

        builder.HasIndex(x => x.UserId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("password_reset_tokens_fk_user")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
