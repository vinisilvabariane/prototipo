using IrisWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IrisWeb.Infrastructure.Mappings;

/// <summary>
/// Sem navegação para <see cref="User"/> de propósito: o filtro de soft delete do usuário não deve
/// esconder o token na hora de revogar.
/// </summary>
public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.TokenHash)
            .HasColumnName("token")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(x => x.ExpiresAt)
            .HasColumnName("expires")
            .IsRequired();

        builder.Property(x => x.Revoked)
            .HasColumnName("revoked")
            .IsRequired();

        builder.Property(x => x.UserId)
            .HasColumnName("fk_user")
            .IsRequired();

        builder.HasIndex(x => x.TokenHash).IsUnique();
    }
}
