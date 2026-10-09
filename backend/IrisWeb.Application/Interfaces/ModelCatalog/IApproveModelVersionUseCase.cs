using FluentResults;
using IrisWeb.Application.Communication.ModelCatalog.Responses;

namespace IrisWeb.Application.Interfaces.ModelCatalog;

public interface IApproveModelVersionUseCase
{
    Task<Result<CatalogModelVersionResponse>> ExecuteAsync(
        Guid modelId,
        Guid versionId,
        CancellationToken cancellationToken);
}
