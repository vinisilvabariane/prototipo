using FluentResults;
using MeOne.Application.Communication.Auth.Requests;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Auth;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Auth;

public sealed class LogoutUseCase(
    IRefreshTokenRepository tokens,
    IUserRepository users,
    IAccessProfileCache accessProfiles,
    ICurrentActor currentActor) : ILogoutUseCase
{
    public async Task<Result> ExecuteAsync(LogoutRequest request, CancellationToken cancellationToken)
    {
        var actor = await AccessGuard.ActorAsync(currentActor, cancellationToken);
        if (actor.IsFailed)
            return actor.ToResult();

        var user = await users.GetWithAccessAsync(request.UserId, cancellationToken);

        if (request.UserId != actor.Value.UserId)
        {
            if (!actor.Value.HasGlobal(Permissions.UsersManage))
                return Result.Fail(new ForbiddenError(AuthErrors.LogoutOtherUserForbidden));

            if (user is null)
                return Result.Fail(new NotFoundError(UserErrors.NotFound));

            var dominance = AccessGuard.EnsureDominates(actor.Value, user);
            if (dominance.IsFailed)
                return dominance;
        }

        await tokens.RevokeAllForUserAsync(request.UserId, cancellationToken);

        if (user is not null)
        {
            user.RotateSecurityStamp();
            users.Update(user);
            await users.SaveChangesAsync(cancellationToken);
        }

        accessProfiles.Invalidate(request.UserId);

        return Result.Ok();
    }
}
