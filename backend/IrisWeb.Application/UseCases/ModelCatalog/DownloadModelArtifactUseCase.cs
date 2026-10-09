using FluentResults;
using IrisWeb.Application.Errors;
using IrisWeb.Application.Interfaces.ModelCatalog;
using IrisWeb.Domain.Enums;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.ModelCatalog;

public sealed class DownloadModelArtifactUseCase(
    IModelCatalogRepository repository,
    IModelArtifactStorage storage) : IDownloadModelArtifactUseCase
{
    public async Task<Result<Stream>> ExecuteAsync(
        Guid modelId,
        Guid versionId,
        CancellationToken cancellationToken)
    {
        var version = await repository.GetVersionAsync(modelId, versionId, cancellationToken);
        if (version is null) return Result.Fail(ModelCatalogErrors.VersionNotFound);

        if (version.Status != ModelVersionStatus.Approved)
            return Result.Fail(ModelCatalogErrors.NotApproved);

        if (version.ArtifactObjectKey is null)
            return Result.Fail(ModelCatalogErrors.ArtifactMissing);

        try
        {
            var content = await storage.OpenReadAsync(version.ArtifactObjectKey, cancellationToken);

            return content is null
                ? Result.Fail(ModelCatalogErrors.ArtifactNotInStorage)
                : Result.Ok(content);
        }
        catch (ModelArtifactStorageException exception)
        {
            return Result.Fail(new UpstreamUnavailableError(exception.Message));
        }
    }
}
