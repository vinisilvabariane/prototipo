using FluentResults;
using MeOne.Application.Communication.Scopes.Responses;

namespace MeOne.Application.Interfaces.Scopes;

public interface IGetScopeUseCase
{
    Task<Result<ScopeResponse>> ExecuteAsync(ulong id, CancellationToken cancellationToken);
}
