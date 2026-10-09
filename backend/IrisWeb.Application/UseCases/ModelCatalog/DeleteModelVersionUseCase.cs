using FluentResults;
using IrisWeb.Application.Errors;
using IrisWeb.Application.Interfaces.ModelCatalog;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.ModelCatalog;

public sealed class DeleteModelVersionUseCase(
    IModelCatalogRepository repository,
    IModelArtifactStorage storage) : IDeleteModelVersionUseCase
{
    public async Task<Result> ExecuteAsync(Guid modelId, Guid versionId, CancellationToken cancellationToken)
    {
        var model = await repository.GetByIdAsync(modelId, cancellationToken);
        if (model is null) return Result.Fail(ModelCatalogErrors.ModelNotFound);

        var version = await repository.GetVersionAsync(modelId, versionId, cancellationToken);
        if (version is null) return Result.Fail(ModelCatalogErrors.VersionNotFound);

        if (model.IsActiveVersion(versionId))
            return Result.Fail(ModelCatalogErrors.CannotDeleteActiveVersion);

        if (version.ArtifactObjectKey is { Length: > 0 } objectKey)
        {
            try
            {
                await storage.DeleteAsync(objectKey, cancellationToken);
            }
            catch (ModelArtifactStorageException exception)
            {
                return Result.Fail(new UpstreamUnavailableError(exception.Message));
            }
        }

        repository.RemoveVersion(version);

        if (await repository.SaveChangesAsync(cancellationToken) <= 0)
            return Result.Fail(ModelCatalogErrors.NothingPersisted);

        return Result.Ok();
    }
}
