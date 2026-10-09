using FluentResults;
using IrisWeb.Application.Errors;
using IrisWeb.Application.Interfaces.ModelCatalog;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.ModelCatalog;

public sealed class DeleteModelUseCase(
    IModelCatalogRepository repository,
    IModelArtifactStorage storage) : IDeleteModelUseCase
{
    public async Task<Result> ExecuteAsync(Guid modelId, CancellationToken cancellationToken)
    {
        var model = await repository.GetByIdAsync(modelId, cancellationToken);
        if (model is null) return Result.Fail(ModelCatalogErrors.ModelNotFound);

        var versions = await repository.GetVersionsAsync(modelId, cancellationToken);

        try
        {
            foreach (var version in versions)
            {
                if (version.ArtifactObjectKey is { Length: > 0 } objectKey)
                    await storage.DeleteAsync(objectKey, cancellationToken);
            }
        }
        catch (ModelArtifactStorageException exception)
        {
            return Result.Fail(new UpstreamUnavailableError(exception.Message));
        }

        repository.RemoveModelWithVersions(model, versions);

        if (await repository.SaveChangesAsync(cancellationToken) <= 0)
            return Result.Fail(ModelCatalogErrors.NothingPersisted);

        return Result.Ok();
    }
}
