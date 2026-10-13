using FluentResults;
using MeOne.Application.Communication.Users.Responses;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces.Users;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Users;

public sealed class GetUserUseCase(IUserRepository users) : IGetUserUseCase
{
    public async Task<Result<UserResponse>> ExecuteAsync(ulong id, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(id, cancellationToken);

        return user is null
            ? Result.Fail(new NotFoundError(UserErrors.NotFound))
            : Result.Ok(UsersMapping.ToResponse(user));
    }
}
