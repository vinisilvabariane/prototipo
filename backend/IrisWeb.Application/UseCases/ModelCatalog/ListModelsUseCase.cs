using FluentResults;
using IrisWeb.Application.Communication.ModelCatalog.Responses;
using IrisWeb.Domain.Interfaces;
using IrisWeb.Application.Interfaces.ModelCatalog;

namespace IrisWeb.Application.UseCases.ModelCatalog;

public sealed class ListModelsUseCase(IModelCatalogRepository repository) : IListModelsUseCase
{
    public async Task<Result<IReadOnlyList<CatalogModelResponse>>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var models = await repository.ListWithVersionsAsync(cancellationToken);

        IReadOnlyList<CatalogModelResponse> response = [.. models.Select(ModelCatalogMapping.ToResponse)];

        return Result.Ok(response);
    }
}
