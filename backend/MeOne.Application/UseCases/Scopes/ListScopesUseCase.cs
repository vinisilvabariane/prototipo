using FluentResults;
using MeOne.Application.Communication.Scopes.Responses;
using MeOne.Application.Interfaces.Scopes;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Scopes;

public sealed class ListScopesUseCase(IScopeRepository scopes) : IListScopesUseCase
{
    public async Task<Result<IReadOnlyList<ScopeResponse>>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var all = await scopes.ListOrderedAsync(cancellationToken);

        return Result.Ok<IReadOnlyList<ScopeResponse>>([.. all.Select(ScopesMapping.ToResponse)]);
    }
}
