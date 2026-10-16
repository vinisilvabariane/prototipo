using FluentResults;

namespace MeOne.Application.Interfaces.Production;

public interface IStartRunUseCase
{
    Task<Result> ExecuteAsync(ulong runId, CancellationToken cancellationToken);
}
