using FluentResults;
using MeOne.Application.Communication.Scopes.Requests;
using MeOne.Application.Communication.Scopes.Responses;

namespace MeOne.Application.Interfaces.Scopes;

public interface ICreateScopeUseCase
{
    Task<Result<ScopeResponse>> ExecuteAsync(CreateScopeRequest request, CancellationToken cancellationToken);
}
