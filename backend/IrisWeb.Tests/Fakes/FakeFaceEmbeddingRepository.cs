using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Tests.Fakes;

public sealed class FakeFaceEmbeddingRepository
    : InMemoryRepository<FaceEmbedding, long>, IFaceEmbeddingRepository
{
    public string? LastRequestedModelVersion { get; private set; }

    public void Seed(string registration, float[] values, string modelVersion = "v1") =>
        Seed(FaceEmbedding.Create(registration, values, modelVersion));

    public Task<IReadOnlyList<FaceEmbedding>> ListByModelVersionAsync(
        string modelVersion,
        CancellationToken cancellationToken)
    {
        LastRequestedModelVersion = modelVersion;

        return Task.FromResult<IReadOnlyList<FaceEmbedding>>(
            [.. Items.Where(item => item.ModelVersion == modelVersion)]);
    }

    public Task<IReadOnlyList<string>> ListRegistrationsAsync(
        string modelVersion,
        CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<string>>(
            [.. Items.Where(item => item.ModelVersion == modelVersion).Select(item => item.Registration).Distinct()]);

    public Task<IReadOnlyList<(string ModelVersion, int Count, DateTime MaxCreatedAt)>> SummarizeByModelVersionAsync(
        CancellationToken cancellationToken)
    {
        IReadOnlyList<(string, int, DateTime)> summary =
            [.. Items
                .GroupBy(item => item.ModelVersion)
                .Select(group => (group.Key, group.Count(), group.Max(item => item.CreatedAt)))];

        return Task.FromResult(summary);
    }
}
