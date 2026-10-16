using FluentResults;

namespace MeOne.Application.Interfaces.Production;

public interface ICloseRunUseCase
{
    Task<Result> ExecuteAsync(ulong runId, CancellationToken cancellationToken);
}
