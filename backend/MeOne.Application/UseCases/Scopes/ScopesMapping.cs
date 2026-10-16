using MeOne.Application.Communication.Scopes.Responses;
using MeOne.Domain.Entities;

namespace MeOne.Application.UseCases.Scopes;

internal static class ScopesMapping
{
    public static ScopeResponse ToResponse(Scope scope) => new(scope.Id, scope.Type, scope.Code, scope.Name, scope.ParentId);

    public static string TypeName(Domain.Enums.ScopeType type) => type.ToString().ToLowerInvariant();
}
