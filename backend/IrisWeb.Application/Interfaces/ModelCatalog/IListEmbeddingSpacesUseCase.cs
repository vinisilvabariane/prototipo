using FluentResults;
using IrisWeb.Application.Communication.ModelCatalog.Responses;

namespace IrisWeb.Application.Interfaces.ModelCatalog;

public interface IListEmbeddingSpacesUseCase
{
    Task<Result<IReadOnlyList<EmbeddingSpaceResponse>>> ExecuteAsync(CancellationToken cancellationToken);
}
