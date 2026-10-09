using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Interfaces;
using IrisWeb.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace IrisWeb.Infrastructure.Repositories;

public sealed class FaceEmbeddingRepository(IrisWebDbContext context)
    : Repository<FaceEmbedding, long>(context), IFaceEmbeddingRepository
{

    public async Task<IReadOnlyList<FaceEmbedding>> ListByModelVersionAsync(
        string modelVersion,
        CancellationToken cancellationToken) =>
        await DbSet
            .AsNoTracking()
            .Where(embedding => embedding.ModelVersion == modelVersion)
            .OrderBy(embedding => embedding.Id)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<string>> ListRegistrationsAsync(
        string modelVersion,
        CancellationToken cancellationToken) =>
        await DbSet
            .AsNoTracking()
            .Where(embedding => embedding.ModelVersion == modelVersion)
            .Select(embedding => embedding.Registration)
            .Distinct()
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<(string ModelVersion, int Count, DateTime MaxCreatedAt)>> SummarizeByModelVersionAsync(
        CancellationToken cancellationToken)
    {
        var rows = await DbSet
            .AsNoTracking()
            .GroupBy(embedding => embedding.ModelVersion)
            .Select(group => new
            {
                ModelVersion = group.Key,
                Count = group.Count(),
                MaxCreatedAt = group.Max(embedding => embedding.CreatedAt),
            })
            .ToListAsync(cancellationToken);

        return [.. rows.Select(row => (row.ModelVersion, row.Count, row.MaxCreatedAt))];
    }
}
