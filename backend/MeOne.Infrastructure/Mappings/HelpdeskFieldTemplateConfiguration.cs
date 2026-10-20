using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeOne.Infrastructure.Mappings;

public sealed class HelpdeskFieldTemplateConfiguration : IEntityTypeConfiguration<HelpdeskFieldTemplate>
{
    public void Configure(EntityTypeBuilder<HelpdeskFieldTemplate> builder)
    {
        builder.ToTable("helpdesk_field_templates");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(HelpdeskErrors.MaxNameLength)
            .IsRequired();

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Label)
            .HasColumnName("label")
            .HasMaxLength(HelpdeskErrors.MaxLabelLength)
            .IsRequired();

        builder.Property(x => x.Required)
            .HasColumnName("required")
            .IsRequired();

        builder.Property(x => x.Options)
            .HasColumnName("options")
            .HasColumnType("longtext")
            .HasConversion(new HelpdeskJson.Converter<IReadOnlyList<HelpdeskOption>>([]), new HelpdeskJson.Comparer<IReadOnlyList<HelpdeskOption>>())
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasConversion<UtcDateTimeConverter>()
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasConversion<UtcDateTimeConverter>()
            .IsRequired();

        builder.HasIndex(x => x.Name).IsUnique();
    }
}
