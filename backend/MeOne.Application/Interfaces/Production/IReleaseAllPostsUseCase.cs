using FluentResults;

namespace MeOne.Application.Interfaces.Production;

public interface IReleaseAllPostsUseCase
{
    Task<Result> ExecuteAsync(ulong runId, CancellationToken cancellationToken);
}
