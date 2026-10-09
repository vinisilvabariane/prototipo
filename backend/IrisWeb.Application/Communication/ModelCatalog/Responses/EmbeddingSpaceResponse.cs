namespace IrisWeb.Application.Communication.ModelCatalog.Responses;

public sealed record EmbeddingSpaceCatalogMatchResponse(
    Guid ModelId,
    string ModelKey,
    Guid VersionId,
    string Version);

public sealed record EmbeddingSpaceResponse(
    string EmbeddingSpace,
    int EnrollmentCount,
    DateTime LastEnrolledAt,
    IReadOnlyList<EmbeddingSpaceCatalogMatchResponse> CatalogMatches);
