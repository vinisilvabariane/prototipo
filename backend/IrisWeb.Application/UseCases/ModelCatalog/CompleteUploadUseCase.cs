using FluentResults;
using IrisWeb.Application.Communication.ModelCatalog.Requests;
using IrisWeb.Application.Communication.ModelCatalog.Responses;
using IrisWeb.Application.Errors;
using IrisWeb.Domain.Enums;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;
using IrisWeb.Application.Interfaces.ModelCatalog;

namespace IrisWeb.Application.UseCases.ModelCatalog;

public sealed class CompleteUploadUseCase(
    IModelCatalogRepository repository,
    IModelArtifactStorage storage) : ICompleteUploadUseCase
{
    public async Task<Result<CatalogModelVersionResponse>> ExecuteAsync(
        Guid modelId,
        Guid versionId,
        CompleteUploadRequest request,
        CancellationToken cancellationToken)
    {
        var version = await repository.GetVersionAsync(modelId, versionId, cancellationToken);
        if (version is null) return Result.Fail(ModelCatalogErrors.VersionNotFound);
        if (version.ArtifactObjectKey is null) return Result.Fail(ModelCatalogErrors.UploadNotStarted);

        if (!version.CanMarkCandidate)
            return Result.Fail(ModelCatalogMapping.InvalidTransition(ModelVersionStatus.Uploading, version.Status));

        var model = await repository.GetWithVersionsAsync(modelId, cancellationToken);
        if (model is null) return Result.Fail(ModelCatalogErrors.ModelNotFound);

        if (ModelCatalogMapping.ManifestViolation(model, request.Manifest) is { } violation)
            return Result.Fail(violation);

        StoredObjectMetadata? metadata;
        try
        {
            metadata = await storage.GetMetadataAsync(version.ArtifactObjectKey, cancellationToken);
        }
        catch (ModelArtifactStorageException exception)
        {
            return Result.Fail(new UpstreamUnavailableError(exception.Message));
        }

        if (metadata is null) return Result.Fail(ModelCatalogErrors.ArtifactNotInStorage);

        if (metadata.Size != request.Size) return Result.Fail(ModelCatalogErrors.ArtifactSizeMismatch);

        try
        {
            version.MarkCandidate(request.Size, request.Sha256, request.Manifest.GetRawText());

            if (await repository.SaveChangesAsync(cancellationToken) <= 0)
                return Result.Fail(ModelCatalogErrors.NothingPersisted);

            return Result.Ok(ModelCatalogMapping.ToResponse(version));
        }
        catch (DomainRuleException exception)
        {
            return Result.Fail(exception.Message);
        }
    }
}
