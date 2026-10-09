using FluentResults;
using IrisWeb.Application.Communication.ModelCatalog.Requests;
using IrisWeb.Application.Communication.ModelCatalog.Responses;

namespace IrisWeb.Application.Interfaces.ModelCatalog;

public interface ICompleteUploadUseCase
{
    Task<Result<CatalogModelVersionResponse>> ExecuteAsync(
        Guid modelId,
        Guid versionId,
        CompleteUploadRequest request,
        CancellationToken cancellationToken);
}
