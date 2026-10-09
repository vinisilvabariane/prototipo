using FluentResults;
using IrisWeb.Application.Communication.ModelCatalog.Responses;

namespace IrisWeb.Application.Interfaces.ModelCatalog;

public interface IGetModelUseCase
{
    Task<Result<CatalogModelResponse>> ExecuteAsync(Guid modelId, CancellationToken cancellationToken);
}
