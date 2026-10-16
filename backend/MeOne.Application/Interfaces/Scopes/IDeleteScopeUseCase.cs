using FluentResults;

namespace MeOne.Application.Interfaces.Scopes;

public interface IDeleteScopeUseCase
{
    Task<Result> ExecuteAsync(ulong id, CancellationToken cancellationToken);
}
