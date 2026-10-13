using FluentResults;
using MeOne.Application.Communication.Users.Requests;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Auth;
using MeOne.Application.Interfaces.Users;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Users;

public sealed class ChangePasswordUseCase(
    IUserRepository users,
    IRefreshTokenRepository tokens,
    IPasswordHasher passwordHasher,
    ICurrentActor currentActor) : IChangePasswordUseCase
{
    public async Task<Result> ExecuteAsync(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return Result.Fail(new NotFoundError(UserErrors.NotFound));

        user.ChangePassword(passwordHasher.Hash(request.NewPassword), UsersMapping.ActorId(currentActor));
        users.Update(user);

        if (await users.SaveChangesAsync(cancellationToken) <= 0)
            return Result.Fail(UserErrors.NothingPersisted);

        await tokens.RevokeAllForUserAsync(user.Id, cancellationToken);

        return Result.Ok();
    }
}
