using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeOne.Infrastructure.Mappings;

public sealed class ScopeConfiguration : IEntityTypeConfiguration<Scope>
{
    public void Configure(EntityTypeBuilder<Scope> builder)
    {
        builder.ToTable("scopes");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(ScopeErrors.MaxCodeLength)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(ScopeErrors.MaxNameLength)
            .IsRequired();

        builder.Property(x => x.ParentId)
            .HasColumnName("fk_parent");

        builder.HasIndex(x => x.Code).IsUnique();

        builder.HasOne<Scope>()
            .WithMany()
            .HasForeignKey(x => x.ParentId)
            .HasConstraintName("scopes_fk_parent")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
