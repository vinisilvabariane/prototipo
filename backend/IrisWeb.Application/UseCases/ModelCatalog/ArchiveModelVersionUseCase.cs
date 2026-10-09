using FluentResults;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;
using IrisWeb.Application.Interfaces.ModelCatalog;

namespace IrisWeb.Application.UseCases.ModelCatalog;

public sealed class ArchiveModelVersionUseCase(IModelCatalogRepository repository) : IArchiveModelVersionUseCase
{
    public async Task<Result> ExecuteAsync(Guid modelId, Guid versionId, CancellationToken cancellationToken)
    {
        var version = await repository.GetVersionAsync(modelId, versionId, cancellationToken);
        if (version is null) return Result.Fail(ModelCatalogErrors.VersionNotFound);

        if (!version.CanArchive)
            return Result.Fail(ModelCatalogErrors.CannotArchiveWhileUploading);

        var model = await repository.GetByIdAsync(modelId, cancellationToken);
        if (model is not null && model.IsActiveVersion(versionId))
            return Result.Fail(ModelCatalogErrors.CannotArchiveActiveVersion);

        var statusAnterior = version.Status;

        try
        {
            version.Archive();

            if (version.Status == statusAnterior)
                return Result.Ok();

            if (await repository.SaveChangesAsync(cancellationToken) <= 0)
                return Result.Fail(ModelCatalogErrors.NothingPersisted);

            return Result.Ok();
        }
        catch (DomainRuleException exception)
        {
            return Result.Fail(exception.Message);
        }
    }
}
