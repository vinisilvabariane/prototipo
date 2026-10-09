using FluentResults;
using IrisWeb.Application.Communication.ModelCatalog.Requests;
using IrisWeb.Application.Communication.ModelCatalog.Responses;

namespace IrisWeb.Application.Interfaces.ModelCatalog;

public interface ISetActiveModelVersionUseCase
{
    Task<Result<CatalogModelResponse>> ExecuteAsync(
        Guid modelId,
        SetActiveVersionRequest request,
        CancellationToken cancellationToken);
}
