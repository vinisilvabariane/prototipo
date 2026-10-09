using FluentResults;

namespace IrisWeb.Application.Interfaces.ModelCatalog;

public interface IDeleteModelVersionUseCase
{
    Task<Result> ExecuteAsync(Guid modelId, Guid versionId, CancellationToken cancellationToken);
}
