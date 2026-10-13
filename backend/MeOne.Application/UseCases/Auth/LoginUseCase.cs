using FluentResults;
using MeOne.Application.Communication.Auth.Requests;
using MeOne.Application.Communication.Auth.Responses;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces.Auth;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Auth;

public sealed class LoginUseCase(
    IUserRepository users,
    IPasswordHasher passwordHasher,
    IAccessTokenIssuer accessTokens,
    IRefreshTokenGenerator refreshTokens,
    IRefreshTokenRepository tokens,
    SessionPolicy policy) : ILoginUseCase
{
    public async Task<Result<AuthResponse>> ExecuteAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await users.GetWithAccessByRegistrationAsync(request.Registration, cancellationToken);

        if (!passwordHasher.Verify(request.Password, user?.PasswordHash) || user is null)
            return Result.Fail(new CredentialRejectedError(AuthErrors.InvalidCredentials));

        await tokens.RevokeAllForUserAsync(user.Id, cancellationToken);

        return await AuthMapping.IssueSessionAsync(user, accessTokens, refreshTokens, tokens, policy, cancellationToken);
    }
}
