using FluentResults;
using MeOne.Application.Communication.Roles.Responses;
using MeOne.Application.Errors;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Roles;

internal static class RolesMapping
{
    public static RoleResponse ToResponse(Role role) => new(
        role.Id,
        role.Name,
        role.Description,
        role.IsAdministrator,
        role.PermissionCodes);

    public static IReadOnlyList<string> Normalize(IEnumerable<string>? permissions) =>
        [.. (permissions ?? []).Select(code => code.Trim()).Distinct(StringComparer.Ordinal)];

    public static string? UnknownPermission(IEnumerable<string> permissions) =>
        permissions.FirstOrDefault(code => !Permissions.Exists(code)) is { } unknown
            ? RoleErrors.UnknownPermission(unknown)
            : null;

    public static async Task<Result<IReadOnlyList<ulong>?>> ScopeChainAsync(
        IScopeRepository scopes,
        ulong? scopeId,
        CancellationToken cancellationToken)
    {
        if (scopeId is not { } id)
            return Result.Ok<IReadOnlyList<ulong>?>(null);

        var chain = await scopes.GetChainAsync(id, cancellationToken);

        return chain.Count == 0
            ? Result.Fail(new NotFoundError(ScopeErrors.NotFound))
            : Result.Ok<IReadOnlyList<ulong>?>(chain);
    }

    public static Result EnsureCanHandle(AccessProfile actor, Role role, IReadOnlyList<ulong>? scopeChain)
    {
        if (role.IsAdministrator && !actor.IsAdministrator)
            return Result.Fail(new ForbiddenError(RoleErrors.BeyondOwnPermissions));

        return actor.Covers(role.PermissionCodes, scopeChain)
            ? Result.Ok()
            : Result.Fail(new ForbiddenError(RoleErrors.BeyondOwnPermissions));
    }
}
