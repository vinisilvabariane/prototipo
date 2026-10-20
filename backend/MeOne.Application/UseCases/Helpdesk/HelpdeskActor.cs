using FluentResults;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.UseCases.Auth;
using MeOne.Domain.Entities;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Helpdesk;

internal sealed record HelpdeskActor(ulong UserId, bool IsAdministrator, IReadOnlyList<ulong> RoleIds, bool CanManage)
{
    public bool CanAttend(HelpdeskSector sector) => sector.CanBeAttendedBy(RoleIds, IsAdministrator);

    public static async Task<Result<HelpdeskActor>> ResolveAsync(
        ICurrentActor currentActor,
        IUserRepository users,
        CancellationToken cancellationToken)
    {
        var actor = await AccessGuard.ActorAsync(currentActor, cancellationToken);
        if (actor.IsFailed)
            return actor.ToResult<HelpdeskActor>();

        var user = await users.GetWithAccessAsync(actor.Value.UserId, cancellationToken);
        var profile = await currentActor.GetAccessProfileAsync(cancellationToken);

        var roleIds = user is null ? [] : user.Roles.Select(link => link.RoleId).Distinct().ToList();

        return Result.Ok(new HelpdeskActor(
            actor.Value.UserId,
            profile?.IsAdministrator ?? false,
            roleIds,
            profile?.Allows(Permissions.HelpdeskManage) ?? false));
    }
}
