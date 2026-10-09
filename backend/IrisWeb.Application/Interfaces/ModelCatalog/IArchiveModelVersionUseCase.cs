using FluentResults;

namespace IrisWeb.Application.Interfaces.ModelCatalog;

public interface IArchiveModelVersionUseCase
{
    Task<Result> ExecuteAsync(Guid modelId, Guid versionId, CancellationToken cancellationToken);
}
