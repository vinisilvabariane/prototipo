using FluentResults;
using IrisWeb.Application.Communication.Users.Responses;
using IrisWeb.Application.Interfaces.Users;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.Users;

/// <summary>Não encontrado responde 400 (e não 404), como no Access Control: o front conta com isso.</summary>
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
