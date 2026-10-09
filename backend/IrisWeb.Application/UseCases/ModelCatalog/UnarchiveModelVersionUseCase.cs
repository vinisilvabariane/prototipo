using FluentResults;
using IrisWeb.Application.Communication.ModelCatalog.Responses;
using IrisWeb.Application.Interfaces.ModelCatalog;
using IrisWeb.Domain.Enums;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.ModelCatalog;

public sealed class UnarchiveModelVersionUseCase(IModelCatalogRepository repository) : IUnarchiveModelVersionUseCase
{
    public async Task<Result<CatalogModelVersionResponse>> ExecuteAsync(
        Guid modelId,
        Guid versionId,
        CancellationToken cancellationToken)
    {
        var version = await repository.GetVersionAsync(modelId, versionId, cancellationToken);
        if (version is null) return Result.Fail(ModelCatalogErrors.VersionNotFound);

        if (!version.CanUnarchive)
            return Result.Fail(ModelCatalogMapping.InvalidTransition(ModelVersionStatus.Archived, version.Status));

        try
        {
            version.Unarchive();

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
