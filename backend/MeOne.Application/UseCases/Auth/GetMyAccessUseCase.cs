using FluentResults;
using MeOne.Application.Communication.Auth.Responses;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Auth;
using MeOne.Application.UseCases.Users;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Auth;

public sealed class GetMyAccessUseCase(IUserRepository users, ICurrentActor currentActor) : IGetMyAccessUseCase
{
    public async Task<Result<MyAccessResponse>> ExecuteAsync(CancellationToken cancellationToken)
    {
        if (currentActor.UserId is not { } userId)
            return Result.Fail(new CredentialRejectedError(AuthErrors.SessionWithoutUser));

        var user = await users.GetWithAccessAsync(userId, cancellationToken);
        if (user is null)
            return Result.Fail(new CredentialRejectedError(AuthErrors.SessionWithoutUser));

        var profile = AccessProfile.From(user);

        return Result.Ok(new MyAccessResponse(
            user.Id,
            user.Registration,
            user.Name,
            user.Email,
            user.Department?.Name ?? string.Empty,
            profile.IsAdministrator,
            [.. user.Roles.Where(link => link.Role is not null).Select(UsersMapping.ToAssignment)],
            [.. profile.Grants.Select(grant => new PermissionGrantResponse(grant.Permission, grant.ScopeId))]));
    }
}
