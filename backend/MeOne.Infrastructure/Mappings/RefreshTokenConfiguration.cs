using MeOne.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeOne.Infrastructure.Mappings;

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

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .HasConstraintName("refresh_tokens_fk_user")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
