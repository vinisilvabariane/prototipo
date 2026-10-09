using IrisWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IrisWeb.Infrastructure.Mappings;

public sealed class ArgosCycleConfiguration : IEntityTypeConfiguration<ArgosCycle>
{
    /// <summary>
    /// Fim do ciclo da peca, o eixo de tempo do dashboard. O model Peewee do desktop nao declara
    /// a coluna; o nome vem do DDL real (`SHOW CREATE TABLE route_verify_chronoanalysis`), onde ela
    /// e indexada (sozinha, com status e com station).
    /// </summary>
    public const string RecordedAtColumn = "finished_at";

    public void Configure(EntityTypeBuilder<ArgosCycle> builder)
    {
        builder.ToTable("route_verify_chronoanalysis");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();

        builder.Property(x => x.Status).HasColumnName("status");
        builder.Property(x => x.CycleSeconds).HasColumnName("cycle_time_s");
        builder.Property(x => x.Station).HasColumnName("station");
        builder.Property(x => x.SetupId).HasColumnName("setup_id");
        builder.Property(x => x.SetupName).HasColumnName("setup_name");
        builder.Property(x => x.FailReason).HasColumnName("fail_reason");
        builder.Property(x => x.FailedPoint).HasColumnName("failed_point");
        builder.Property(x => x.MinConfidence).HasColumnName("min_confidence");
        builder.Property(x => x.RecordedAt).HasColumnName(RecordedAtColumn);

        builder.Ignore(x => x.IsOk);
    }
}
