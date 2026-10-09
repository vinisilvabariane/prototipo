using FluentResults;
using IrisWeb.Application.Communication.Users.Requests;
using IrisWeb.Application.Errors;
using IrisWeb.Application.Interfaces;
using IrisWeb.Application.Interfaces.Auth;
using IrisWeb.Application.Interfaces.Users;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.Users;

/// <summary>Master define a senha de um usuário; as sessões abertas dele caem.</summary>
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
