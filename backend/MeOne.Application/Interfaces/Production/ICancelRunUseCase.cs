using FluentResults;

namespace MeOne.Application.Interfaces.Production;

public interface ICancelRunUseCase
{
    Task<Result> ExecuteAsync(ulong runId, CancellationToken cancellationToken);
}
