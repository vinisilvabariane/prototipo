using MeOne.API.Auth;
using MeOne.API.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MeOne.Application.Communication.Auth.Responses;
using MeOne.Application.Communication.Auth.Requests;
using MeOne.Application.Interfaces.Auth;

namespace MeOne.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(
    ILoginUseCase login,
    IRefreshSessionUseCase refreshSession,
    ILogoutUseCase logout,
    IValidateCredentialsUseCase validateCredentials) : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    [EnableRateLimiting(AuthPolicies.AnonymousRateLimit)]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AuthResponse>> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await login.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Não foi possível autenticar", result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Refresh(
        RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await refreshSession.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Não foi possível renovar a sessão", result);
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Logout(
        LogoutRequest request,
        CancellationToken cancellationToken)
    {
        var result = await logout.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Não foi possível encerrar a sessão", result);
    }

    [HttpPost("valid")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthPolicies.AnonymousRateLimit)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Valid(
        ValidateCredentialsRequest request,
        CancellationToken cancellationToken)
    {
        var result = await validateCredentials.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess ? Ok() : ApiResults.Failure("Credenciais inválidas", result);
    }
}
