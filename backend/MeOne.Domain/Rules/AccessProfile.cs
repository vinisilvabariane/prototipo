using MeOne.Domain.Entities;

namespace MeOne.Domain.Rules;

public sealed record AccessGrant(string Permission, ulong? ScopeId);

public sealed class AccessProfile
{
    private readonly IReadOnlyList<AccessGrant> _grants;

    public AccessProfile(ulong userId, bool active, string securityStamp, bool isAdministrator, IEnumerable<AccessGrant> grants)
    {
        UserId = userId;
        Active = active;
        SecurityStamp = securityStamp;
        IsAdministrator = isAdministrator;
        _grants = isAdministrator
            ? [.. Permissions.All.Select(permission => new AccessGrant(permission.Code, null))]
            : [.. grants.Distinct()];
    }

    public ulong UserId { get; }

    public bool Active { get; }

    public string SecurityStamp { get; }

    public bool IsAdministrator { get; }

    public IReadOnlyList<AccessGrant> Grants => _grants;

    public IReadOnlySet<string> AllPermissions => _grants.Select(grant => grant.Permission).ToHashSet(StringComparer.Ordinal);

    public static AccessProfile From(User user)
    {
        var links = user.Roles.Where(link => link.Role is not null).ToList();

        return new AccessProfile(
            user.Id,
            user.Active,
            user.SecurityStamp,
            links.Any(link => link.Role!.IsAdministrator && link.ScopeId is null),
            links.SelectMany(link => link.Role!.PermissionCodes.Select(code => new AccessGrant(code, link.ScopeId))));
    }

    public bool HasGlobal(string permission) =>
        _grants.Any(grant => grant.Permission == permission && grant.ScopeId is null);

    public bool HasAnywhere(string permission) =>
        _grants.Any(grant => grant.Permission == permission);

    public bool HasIn(string permission, IReadOnlyCollection<ulong> scopeChain) =>
        _grants.Any(grant => grant.Permission == permission &&
                             (grant.ScopeId is null || scopeChain.Contains(grant.ScopeId.Value)));

    public bool Allows(string permission) =>
        Permissions.IsGlobalOnly(permission) ? HasGlobal(permission) : HasAnywhere(permission);

    public bool Covers(IEnumerable<string> permissions, IReadOnlyCollection<ulong>? scopeChain) =>
        permissions.All(permission => scopeChain is null ? HasGlobal(permission) : HasIn(permission, scopeChain));

    public bool Dominates(AccessProfile target) =>
        IsAdministrator ||
        (!target.IsAdministrator && target.AllPermissions.All(HasGlobal));
}
