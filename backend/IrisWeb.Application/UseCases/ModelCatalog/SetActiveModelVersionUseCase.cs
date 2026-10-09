using FluentResults;
using IrisWeb.Application.Communication.ModelCatalog.Requests;
using IrisWeb.Application.Communication.ModelCatalog.Responses;
using IrisWeb.Application.Interfaces.ModelCatalog;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.ModelCatalog;

public sealed class SetActiveModelVersionUseCase(IModelCatalogRepository repository) : ISetActiveModelVersionUseCase
{
    public async Task<Result<CatalogModelResponse>> ExecuteAsync(
        Guid modelId,
        SetActiveVersionRequest request,
        CancellationToken cancellationToken)
    {
        var model = await repository.GetWithVersionsAsync(modelId, cancellationToken);
        if (model is null) return Result.Fail(ModelCatalogErrors.ModelNotFound);

        var version = await repository.GetVersionAsync(modelId, request.VersionId, cancellationToken);
        if (version is null) return Result.Fail(ModelCatalogErrors.VersionNotFound);

        if (model.IsActiveVersion(version.Id))
            return Result.Ok(ModelCatalogMapping.ToResponse(model));

        try
        {
            model.SetActiveVersion(version);
            repository.Update(model);

            if (await repository.SaveChangesAsync(cancellationToken) <= 0)
                return Result.Fail(ModelCatalogErrors.NothingPersisted);

            return Result.Ok(ModelCatalogMapping.ToResponse(model));
        }
        catch (DomainRuleException exception)
        {
            return Result.Fail(exception.Message);
        }
    }
}
