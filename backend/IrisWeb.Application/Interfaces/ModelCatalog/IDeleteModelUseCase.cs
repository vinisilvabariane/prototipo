using FluentResults;

namespace IrisWeb.Application.Interfaces.ModelCatalog;

public interface IDeleteModelUseCase
{
    Task<Result> ExecuteAsync(Guid modelId, CancellationToken cancellationToken);
}
