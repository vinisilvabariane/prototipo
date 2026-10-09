using FluentResults;
using IrisWeb.Application.Errors;
using IrisWeb.Application.Interfaces.ModelCatalog;
using IrisWeb.Domain.Enums;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.ModelCatalog;

public sealed class UploadModelArtifactUseCase(
    IModelCatalogRepository repository,
    IModelArtifactStorage storage) : IUploadModelArtifactUseCase
{
    public async Task<Result> ExecuteAsync(
        Guid modelId,
        Guid versionId,
        Stream content,
        CancellationToken cancellationToken)
    {
        var version = await repository.GetVersionAsync(modelId, versionId, cancellationToken);
        if (version is null) return Result.Fail(ModelCatalogErrors.VersionNotFound);
        if (version.ArtifactObjectKey is null) return Result.Fail(ModelCatalogErrors.UploadNotStarted);

        if (!version.CanMarkCandidate)
            return Result.Fail(ModelCatalogMapping.InvalidTransition(ModelVersionStatus.Uploading, version.Status));

        try
        {
            await storage.SaveAsync(version.ArtifactObjectKey, content, cancellationToken);
            return Result.Ok();
        }
        catch (ModelArtifactStorageException exception)
        {
            return Result.Fail(new UpstreamUnavailableError(exception.Message));
        }
    }
}
