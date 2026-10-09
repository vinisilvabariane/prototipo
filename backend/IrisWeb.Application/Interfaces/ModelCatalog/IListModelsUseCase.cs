using FluentResults;
using IrisWeb.Application.Communication.ModelCatalog.Responses;

namespace IrisWeb.Application.Interfaces.ModelCatalog;

public interface IListModelsUseCase
{
    Task<Result<IReadOnlyList<CatalogModelResponse>>> ExecuteAsync(CancellationToken cancellationToken);
}
