using FluentResults;
using IrisWeb.Application.Communication.Users.Responses;
using IrisWeb.Application.Errors;
using IrisWeb.Application.Interfaces.Users;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.Users;

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
