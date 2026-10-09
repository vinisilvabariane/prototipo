using FluentResults;

namespace IrisWeb.Application.Interfaces.ModelCatalog;

public interface IDownloadModelArtifactUseCase
{
    Task<Result<Stream>> ExecuteAsync(Guid modelId, Guid versionId, CancellationToken cancellationToken);
}
