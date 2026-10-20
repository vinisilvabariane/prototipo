using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Helpdesk;

internal static class HelpdeskRoleCheck
{
    public static async Task<bool> UnknownAsync(IRoleRepository roles, IReadOnlyList<ulong> roleIds, CancellationToken cancellationToken)
    {
        if (roleIds.Count == 0)
            return false;

        var known = (await roles.ListOrderedAsync(cancellationToken)).Select(role => role.Id).ToHashSet();

        return roleIds.Any(id => !known.Contains(id));
    }
}
