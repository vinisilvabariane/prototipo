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
        var now = DateTime.UtcNow;
        var user = await users.GetWithAccessByRegistrationAsync(request.Registration, cancellationToken);
        var passwordMatches = passwordHasher.Verify(request.Password, user?.PasswordHash);

        if (user is null || user.IsLockedOut(now))
            return Result.Fail(new CredentialRejectedError(AuthErrors.InvalidCredentials));

        if (!passwordMatches)
        {
            user.RegisterFailedLogin(now);
            users.Update(user);
            await users.SaveChangesAsync(cancellationToken);

            return Result.Fail(new CredentialRejectedError(AuthErrors.InvalidCredentials));
        }

        if (user.RegisterSuccessfulLogin())
        {
            users.Update(user);
            await users.SaveChangesAsync(cancellationToken);
        }

        await tokens.RevokeAllForUserAsync(user.Id, cancellationToken);

        return await AuthMapping.IssueSessionAsync(user, accessTokens, refreshTokens, tokens, policy, cancellationToken);
    }
}
