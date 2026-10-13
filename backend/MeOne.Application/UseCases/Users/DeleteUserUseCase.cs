using FluentResults;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Users;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Users;

public sealed class DeleteUserUseCase(
    IUserRepository users,
    IRefreshTokenRepository tokens,
    ICurrentActor currentActor) : IDeleteUserUseCase
{
    public async Task<Result> ExecuteAsync(ulong id, CancellationToken cancellationToken)
    {
        if (currentActor.UserId == id)
            return Result.Fail(UserErrors.CannotDeleteSelf);

        var user = await users.GetByIdAsync(id, cancellationToken);
        if (user is null)
            return Result.Fail(new NotFoundError(UserErrors.NotFound));

        user.Deactivate(UsersMapping.ActorId(currentActor));
        users.Update(user);

        if (await users.SaveChangesAsync(cancellationToken) <= 0)
            return Result.Fail(UserErrors.NothingPersisted);

        await tokens.RevokeAllForUserAsync(id, cancellationToken);

        return Result.Ok();
    }
}
