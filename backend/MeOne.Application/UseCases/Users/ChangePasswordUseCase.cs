using FluentResults;
using MeOne.Application.Communication.Users.Requests;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Auth;
using MeOne.Application.Interfaces.Users;
using MeOne.Application.UseCases.Auth;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Users;

public sealed class ChangePasswordUseCase(
    IUserRepository users,
    IRefreshTokenRepository tokens,
    IPasswordHasher passwordHasher,
    IAccessProfileCache accessProfiles,
    ICurrentActor currentActor) : IChangePasswordUseCase
{
    public async Task<Result> ExecuteAsync(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var actor = await AccessGuard.ActorAsync(currentActor, cancellationToken);
        if (actor.IsFailed)
            return actor.ToResult();

        var user = await users.GetWithAccessAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result.Fail(new NotFoundError(UserErrors.NotFound));

        var dominance = AccessGuard.EnsureDominates(actor.Value, user);
        if (dominance.IsFailed)
            return dominance;

        user.ChangePassword(passwordHasher.Hash(request.NewPassword), actor.Value.UserId);
        users.Update(user);

        if (await users.SaveChangesAsync(cancellationToken) <= 0)
            return Result.Fail(UserErrors.NothingPersisted);

        await tokens.RevokeAllForUserAsync(user.Id, cancellationToken);
        accessProfiles.Invalidate(user.Id);

        return Result.Ok();
    }
}
