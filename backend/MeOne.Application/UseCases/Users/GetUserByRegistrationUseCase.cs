using FluentResults;
using MeOne.Application.Communication.Users.Responses;
using MeOne.Application.Interfaces.Users;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Users;

public sealed class GetUserByRegistrationUseCase(IUserRepository users) : IGetUserByRegistrationUseCase
{
    public async Task<Result<UserResponse>> ExecuteAsync(uint registration, CancellationToken cancellationToken)
    {
        var user = await users.GetByRegistrationAsync(registration, cancellationToken);

        return user is null
            ? Result.Fail(UserErrors.NotFound)
            : Result.Ok(UsersMapping.ToResponse(user));
    }
}
