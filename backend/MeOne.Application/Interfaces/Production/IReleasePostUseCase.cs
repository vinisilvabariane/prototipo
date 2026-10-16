using FluentResults;

namespace MeOne.Application.Interfaces.Production;

public interface IReleasePostUseCase
{
    Task<Result> ExecuteAsync(ulong runId, ulong postId, CancellationToken cancellationToken);
}
