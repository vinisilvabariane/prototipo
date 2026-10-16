using FluentResults;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces.Scopes;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Scopes;

public sealed class DeleteScopeUseCase(IScopeRepository scopes) : IDeleteScopeUseCase
{
    public async Task<Result> ExecuteAsync(ulong id, CancellationToken cancellationToken)
    {
        var scope = await scopes.GetByIdAsync(id, cancellationToken);
        if (scope is null)
            return Result.Fail(new NotFoundError(ScopeErrors.NotFound));

        if (await scopes.HasChildrenAsync(id, cancellationToken))
            return Result.Fail(ScopeErrors.HasChildren);

        if (await scopes.InUseAsync(id, cancellationToken))
            return Result.Fail(ScopeErrors.InUse);

        scopes.Delete(scope);

        return await scopes.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok()
            : Result.Fail(ScopeErrors.NothingPersisted);
    }
}
