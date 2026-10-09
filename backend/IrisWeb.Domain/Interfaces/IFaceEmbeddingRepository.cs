using IrisWeb.Domain.Entities;

namespace IrisWeb.Domain.Interfaces;

public interface IFaceEmbeddingRepository : IRepository<FaceEmbedding, long>
{

    Task<IReadOnlyList<FaceEmbedding>> ListByModelVersionAsync(
        string modelVersion,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> ListRegistrationsAsync(
        string modelVersion,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<(string ModelVersion, int Count, DateTime MaxCreatedAt)>> SummarizeByModelVersionAsync(
        CancellationToken cancellationToken);
}
