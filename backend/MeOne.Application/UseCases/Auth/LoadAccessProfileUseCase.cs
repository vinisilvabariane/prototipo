using FluentResults;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces.Auth;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Auth;

public sealed class LoadAccessProfileUseCase(IUserRepository users) : ILoadAccessProfileUseCase
{
    public async Task<Result<AccessProfile>> ExecuteAsync(ulong userId, CancellationToken cancellationToken)
    {
        var user = await users.GetWithAccessAsync(userId, cancellationToken);

        return user is null
            ? Result.Fail(new NotFoundError(UserErrors.NotFound))
            : Result.Ok(AccessProfile.From(user));
    }
}
