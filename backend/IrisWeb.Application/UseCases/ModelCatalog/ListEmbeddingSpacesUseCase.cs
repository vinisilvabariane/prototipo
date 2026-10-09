using FluentResults;
using IrisWeb.Application.Communication.ModelCatalog.Responses;
using IrisWeb.Application.Interfaces.ModelCatalog;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.ModelCatalog;

public sealed class ListEmbeddingSpacesUseCase(
    IFaceEmbeddingRepository embeddings,
    IModelCatalogRepository catalog) : IListEmbeddingSpacesUseCase
{
    public async Task<Result<IReadOnlyList<EmbeddingSpaceResponse>>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var summaries = await embeddings.SummarizeByModelVersionAsync(cancellationToken);
        var models = await catalog.ListWithVersionsAsync(cancellationToken);

        var versionsBySpace = models
            .SelectMany(model => model.Versions.Select(version => (model, version)))
            .Select(pair => (pair.model, pair.version, space: ModelCatalogMapping.ResolveEmbeddingSpace(pair.version)))
            .Where(entry => entry.space is not null)
            .ToLookup(entry => entry.space!);

        IReadOnlyList<EmbeddingSpaceResponse> response = [.. summaries
            .OrderByDescending(summary => summary.MaxCreatedAt)
            .Select(summary => new EmbeddingSpaceResponse(
                summary.ModelVersion,
                summary.Count,
                summary.MaxCreatedAt,
                [.. versionsBySpace[summary.ModelVersion]
                    .Select(entry => new EmbeddingSpaceCatalogMatchResponse(
                        entry.model.Id, entry.model.Key, entry.version.Id, entry.version.Version))]))];

        return Result.Ok(response);
    }
}
