using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeOne.Infrastructure.Mappings;

public sealed class HelpdeskSectorConfiguration : IEntityTypeConfiguration<HelpdeskSector>
{
    public void Configure(EntityTypeBuilder<HelpdeskSector> builder)
    {
        builder.ToTable("helpdesk_sectors");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(HelpdeskErrors.MaxNameLength)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(HelpdeskErrors.MaxDescriptionLength);

        builder.Property(x => x.ColorHex)
            .HasColumnName("color_hex")
            .HasMaxLength(7)
            .IsRequired();

        builder.Property(x => x.Icon)
            .HasColumnName("icon")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.Badges)
            .HasColumnName("badges")
            .HasColumnType("longtext")
            .HasConversion(new HelpdeskJson.Converter<IReadOnlyList<string>>([]), new HelpdeskJson.Comparer<IReadOnlyList<string>>())
            .IsRequired();

        builder.Property(x => x.Active)
            .HasColumnName("active")
            .IsRequired();

        builder.Property(x => x.ExternalUrl)
            .HasColumnName("external_url")
            .HasMaxLength(HelpdeskErrors.MaxUrlLength);

        builder.Property(x => x.Form)
            .HasColumnName("form_schema")
            .HasColumnType("longtext")
            .HasConversion(new HelpdeskJson.Converter<HelpdeskForm>(HelpdeskForm.Empty), new HelpdeskJson.Comparer<HelpdeskForm>())
            .IsRequired();

        builder.Property(x => x.ResolutionForm)
            .HasColumnName("resolution_schema")
            .HasColumnType("longtext")
            .HasConversion(new HelpdeskJson.Converter<HelpdeskForm>(HelpdeskForm.Empty), new HelpdeskJson.Comparer<HelpdeskForm>())
            .IsRequired();

        builder.Property(x => x.SummaryFields)
            .HasColumnName("summary_fields")
            .HasColumnType("longtext")
            .HasConversion(new HelpdeskJson.Converter<IReadOnlyList<string>>([]), new HelpdeskJson.Comparer<IReadOnlyList<string>>())
            .IsRequired();

        builder.Property(x => x.AttendantRoleIds)
            .HasColumnName("attendant_role_ids")
            .HasColumnType("longtext")
            .HasConversion(new HelpdeskJson.Converter<IReadOnlyList<ulong>>([]), new HelpdeskJson.Comparer<IReadOnlyList<ulong>>())
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasConversion<UtcDateTimeConverter>()
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasConversion<UtcDateTimeConverter>()
            .IsRequired();

        builder.Ignore(x => x.IsExternal);
        builder.Ignore(x => x.HasResolutionForm);
        builder.HasIndex(x => x.Name).IsUnique();
    }
}
