using FluentResults;
using IrisWeb.Application.Communication.ModelCatalog.Requests;
using IrisWeb.Application.Communication.ModelCatalog.Responses;

namespace IrisWeb.Application.Interfaces.ModelCatalog;

public interface ICreateModelVersionUseCase
{
    Task<Result<CatalogModelVersionResponse>> ExecuteAsync(
        Guid modelId,
        CreateModelVersionRequest request,
        CancellationToken cancellationToken);
}
