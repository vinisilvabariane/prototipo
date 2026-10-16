using FluentResults;

namespace MeOne.Application.Interfaces.Production;

public interface IDeleteLayoutUseCase
{
    Task<Result> ExecuteAsync(ulong id, CancellationToken cancellationToken);
}
