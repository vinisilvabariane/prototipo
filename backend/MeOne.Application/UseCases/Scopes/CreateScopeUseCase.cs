using FluentResults;
using MeOne.Application.Communication.Scopes.Requests;
using MeOne.Application.Communication.Scopes.Responses;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces.Scopes;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Scopes;

public sealed class CreateScopeUseCase(IScopeRepository scopes) : ICreateScopeUseCase
{
    public async Task<Result<ScopeResponse>> ExecuteAsync(CreateScopeRequest request, CancellationToken cancellationToken)
    {
        var parentType = ScopeHierarchy.ParentTypeOf(request.Type);

        if (parentType is null && request.ParentId is not null)
            return Result.Fail(ScopeErrors.PlantHasNoParent);

        if (parentType is { } expected)
        {
            if (request.ParentId is not { } parentId)
                return Result.Fail(ScopeErrors.ParentRequired(ScopesMapping.TypeName(expected)));

            var parent = await scopes.GetByIdAsync(parentId, cancellationToken);
            if (parent is null)
                return Result.Fail(new NotFoundError(ScopeErrors.NotFound));

            if (parent.Type != expected)
                return Result.Fail(ScopeErrors.ParentMustBe(ScopesMapping.TypeName(expected)));
        }

        var scope = Scope.Create(request.Type, request.Code, request.Name, request.ParentId);

        if (await scopes.CodeExistsAsync(scope.Code, exceptScopeId: null, cancellationToken))
            return Result.Fail(ScopeErrors.CodeTaken(scope.Code));

        await scopes.AddAsync(scope, cancellationToken);

        if (await scopes.SaveChangesAsync(cancellationToken) <= 0)
            return Result.Fail(ScopeErrors.NothingPersisted);

        return Result.Ok(ScopesMapping.ToResponse(scope));
    }
}
