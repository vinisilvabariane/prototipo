using FluentResults;
using MeOne.Application.Communication.Scopes.Requests;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces.Scopes;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Scopes;

public sealed class UpdateScopeUseCase(IScopeRepository scopes) : IUpdateScopeUseCase
{
    public async Task<Result> ExecuteAsync(ulong id, UpdateScopeRequest request, CancellationToken cancellationToken)
    {
        var scope = await scopes.GetByIdAsync(id, cancellationToken);
        if (scope is null)
            return Result.Fail(new NotFoundError(ScopeErrors.NotFound));

        var code = Scope.NormalizeCode(request.Code);

        if (await scopes.CodeExistsAsync(code, exceptScopeId: id, cancellationToken))
            return Result.Fail(ScopeErrors.CodeTaken(code));

        if (!scope.Update(request.Code, request.Name))
            return Result.Ok();

        scopes.Update(scope);

        return await scopes.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok()
            : Result.Fail(ScopeErrors.NothingPersisted);
    }
}
