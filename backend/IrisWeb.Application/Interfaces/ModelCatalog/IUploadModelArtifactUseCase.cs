using FluentResults;

namespace IrisWeb.Application.Interfaces.ModelCatalog;

public interface IUploadModelArtifactUseCase
{
    Task<Result> ExecuteAsync(Guid modelId, Guid versionId, Stream content, CancellationToken cancellationToken);
}
