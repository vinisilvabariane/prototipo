using FluentResults;
using MeOne.Application.Communication.Scopes.Requests;

namespace MeOne.Application.Interfaces.Scopes;

public interface IUpdateScopeUseCase
{
    Task<Result> ExecuteAsync(ulong id, UpdateScopeRequest request, CancellationToken cancellationToken);
}
