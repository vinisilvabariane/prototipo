using FluentResults;
using MeOne.Application.Communication.Scopes.Responses;

namespace MeOne.Application.Interfaces.Scopes;

public interface IListScopesUseCase
{
    Task<Result<IReadOnlyList<ScopeResponse>>> ExecuteAsync(CancellationToken cancellationToken);
}
