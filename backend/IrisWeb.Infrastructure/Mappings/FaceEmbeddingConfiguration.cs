using IrisWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace IrisWeb.Infrastructure.Mappings;

public sealed class FaceEmbeddingConfiguration : IEntityTypeConfiguration<FaceEmbedding>
{

    private static readonly ValueConverter<float[], byte[]> BlobConverter = new(
        values => FaceEmbeddingBlob.ToBytes(values),
        blob => FaceEmbeddingBlob.ToFloats(blob));

    private static readonly ValueComparer<float[]> BlobComparer = new(
        (left, right) => left != null && right != null && left.SequenceEqual(right),
        values => values.Aggregate(0, (hash, value) => HashCode.Combine(hash, value)),
        values => values.ToArray());

    public void Configure(EntityTypeBuilder<FaceEmbedding> builder)
    {
        builder.ToTable("face_detector_embeddings");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedOnAdd();

        builder.Property(x => x.Registration)
            .HasColumnName("registration")
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(x => x.Values)
            .HasColumnName("embedding")
            .HasColumnType("blob")
            .HasConversion(BlobConverter, BlobComparer)
            .IsRequired();

        builder.Property(x => x.Dimension)
            .HasColumnName("embedding_dim")
            .IsRequired();

        builder.Property(x => x.ModelVersion)
            .HasColumnName("model_version")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .ValueGeneratedOnAdd();

        builder.HasIndex(x => x.Registration).HasDatabaseName("idx_registration");
    }
}
