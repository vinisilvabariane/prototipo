using FluentResults;
using MeOne.Application.Communication.Scopes.Responses;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces.Scopes;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Scopes;

public sealed class GetScopeUseCase(IScopeRepository scopes) : IGetScopeUseCase
{
    public async Task<Result<ScopeResponse>> ExecuteAsync(ulong id, CancellationToken cancellationToken)
    {
        var scope = await scopes.GetByIdAsync(id, cancellationToken);

        return scope is null
            ? Result.Fail(new NotFoundError(ScopeErrors.NotFound))
            : Result.Ok(ScopesMapping.ToResponse(scope));
    }
}
