using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeOne.Infrastructure.Mappings;

public sealed class HelpdeskTicketConfiguration : IEntityTypeConfiguration<HelpdeskTicket>
{
    public void Configure(EntityTypeBuilder<HelpdeskTicket> builder)
    {
        builder.ToTable("helpdesk_tickets");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        builder.Property(x => x.SectorId).HasColumnName("sector_id").IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.OpenedBy).HasColumnName("opened_by").IsRequired();

        builder.Property(x => x.OpenedAt)
            .HasColumnName("opened_at")
            .HasConversion<UtcDateTimeConverter>()
            .IsRequired();

        builder.Property(x => x.LineId).HasColumnName("line_id");
        builder.Property(x => x.PostId).HasColumnName("post_id");
        builder.Property(x => x.PostName).HasColumnName("post_name").HasMaxLength(HelpdeskErrors.MaxNameLength);
        builder.Property(x => x.IsLineStopped).HasColumnName("is_line_stopped").IsRequired();

        builder.Property(x => x.StoppedAt)
            .HasColumnName("stopped_at")
            .HasConversion<UtcNullableDateTimeConverter>();

        builder.Property(x => x.Defects).HasColumnName("defects").IsRequired();

        builder.Property(x => x.FormSnapshot)
            .HasColumnName("form_snapshot")
            .HasColumnType("longtext")
            .HasConversion(new HelpdeskJson.Converter<HelpdeskForm>(HelpdeskForm.Empty), new HelpdeskJson.Comparer<HelpdeskForm>())
            .IsRequired();

        builder.Property(x => x.Answers)
            .HasColumnName("answers")
            .HasColumnType("longtext")
            .HasConversion(
                new HelpdeskJson.Converter<IReadOnlyDictionary<string, string>>(new Dictionary<string, string>()),
                new HelpdeskJson.Comparer<IReadOnlyDictionary<string, string>>())
            .IsRequired();

        builder.Property(x => x.AssignedTo).HasColumnName("assigned_to");

        builder.Property(x => x.StartedAt)
            .HasColumnName("started_at")
            .HasConversion<UtcNullableDateTimeConverter>();

        builder.Property(x => x.ResolutionSnapshot)
            .HasColumnName("resolution_snapshot")
            .HasColumnType("longtext")
            .HasConversion(new HelpdeskJson.Converter<HelpdeskForm>(HelpdeskForm.Empty), new HelpdeskJson.Comparer<HelpdeskForm>())
            .IsRequired();

        builder.Property(x => x.ResolutionAnswers)
            .HasColumnName("resolution_answers")
            .HasColumnType("longtext")
            .HasConversion(
                new HelpdeskJson.Converter<IReadOnlyDictionary<string, string>>(new Dictionary<string, string>()),
                new HelpdeskJson.Comparer<IReadOnlyDictionary<string, string>>())
            .IsRequired();

        builder.Property(x => x.Note).HasColumnName("note").HasMaxLength(HelpdeskErrors.MaxNoteLength);

        builder.Property(x => x.FinishedAt)
            .HasColumnName("finished_at")
            .HasConversion<UtcNullableDateTimeConverter>();

        builder.Property(x => x.RejectedBy).HasColumnName("rejected_by");
        builder.Property(x => x.RejectReason).HasColumnName("reject_reason").HasMaxLength(HelpdeskErrors.MaxReasonLength);

        builder.Ignore(x => x.IsClosed);
        builder.Ignore(x => x.DowntimeStart);

        builder.HasIndex(x => new { x.Status, x.OpenedAt });
        builder.HasIndex(x => new { x.SectorId, x.Status });
        builder.HasIndex(x => new { x.LineId, x.StoppedAt });
    }
}
