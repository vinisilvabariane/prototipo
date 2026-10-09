using FluentResults;
using IrisWeb.Application.Communication.ModelCatalog.Responses;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;
using IrisWeb.Application.Interfaces.ModelCatalog;

namespace IrisWeb.Application.UseCases.ModelCatalog;

public sealed class GetModelUseCase(IModelCatalogRepository repository) : IGetModelUseCase
{
    public async Task<Result<CatalogModelResponse>> ExecuteAsync(Guid modelId, CancellationToken cancellationToken)
    {
        var model = await repository.GetWithVersionsAsync(modelId, cancellationToken);

        return model is null
            ? Result.Fail(ModelCatalogErrors.ModelNotFound)
            : Result.Ok(ModelCatalogMapping.ToResponse(model));
    }
}
