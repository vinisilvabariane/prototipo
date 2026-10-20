using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeOne.Infrastructure.Mappings;

public sealed class OeeFormulaSetConfiguration : IEntityTypeConfiguration<OeeFormulaSet>
{
    public void Configure(EntityTypeBuilder<OeeFormulaSet> builder)
    {
        builder.ToTable("oee_formula_sets");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(OeeErrors.MaxSetNameLength)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        Formula(builder, x => x.Availability, "availability");
        Formula(builder, x => x.Performance, "performance");
        Formula(builder, x => x.Quality, "quality");
        Formula(builder, x => x.MachineOee, "machine_oee");
        Formula(builder, x => x.LineAvailability, "line_availability");
        Formula(builder, x => x.LinePerformance, "line_performance");
        Formula(builder, x => x.LineQuality, "line_quality");
        Formula(builder, x => x.LineOee, "line_oee");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .HasConversion<UtcDateTimeConverter>()
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at")
            .HasConversion<UtcDateTimeConverter>()
            .IsRequired();

        builder.Property(x => x.UpdatedBy)
            .HasColumnName("updated_by")
            .IsRequired();

        builder.HasIndex(x => x.Name).IsUnique();
        builder.Ignore(x => x.Formulas);
    }

    private static void Formula(
        EntityTypeBuilder<OeeFormulaSet> builder,
        System.Linq.Expressions.Expression<Func<OeeFormulaSet, string>> property,
        string column) =>
        builder.Property(property)
            .HasColumnName(column)
            .HasMaxLength(OeeErrors.MaxFormulaLength)
            .IsRequired();
}
