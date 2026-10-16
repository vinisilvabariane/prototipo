using FluentResults;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Auth;
using MeOne.Application.Interfaces.Users;
using MeOne.Application.UseCases.Auth;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Users;

public sealed class DeleteUserUseCase(
    IUserRepository users,
    IRefreshTokenRepository tokens,
    IAccessProfileCache accessProfiles,
    ICurrentActor currentActor) : IDeleteUserUseCase
{
    public async Task<Result> ExecuteAsync(ulong id, CancellationToken cancellationToken)
    {
        var actor = await AccessGuard.ActorAsync(currentActor, cancellationToken);
        if (actor.IsFailed)
            return actor.ToResult();

        if (actor.Value.UserId == id)
            return Result.Fail(UserErrors.CannotDeleteSelf);

        var user = await users.GetWithAccessAsync(id, cancellationToken);
        if (user is null)
            return Result.Fail(new NotFoundError(UserErrors.NotFound));

        var dominance = AccessGuard.EnsureDominates(actor.Value, user);
        if (dominance.IsFailed)
            return dominance;

        var lastAdministrator = await AccessGuard.EnsureNotLastAdministratorAsync(users, user, cancellationToken);
        if (lastAdministrator.IsFailed)
            return lastAdministrator;

        user.Deactivate(actor.Value.UserId);
        users.Update(user);

        if (await users.SaveChangesAsync(cancellationToken) <= 0)
            return Result.Fail(UserErrors.NothingPersisted);

        await tokens.RevokeAllForUserAsync(id, cancellationToken);
        accessProfiles.Invalidate(id);

        return Result.Ok();
    }
}
