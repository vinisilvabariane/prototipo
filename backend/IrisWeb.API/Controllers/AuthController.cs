using IrisWeb.API.Auth;
using IrisWeb.API.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using IrisWeb.Application.Communication.Auth.Responses;
using IrisWeb.Application.Communication.Auth.Requests;
using IrisWeb.Application.Interfaces.Auth;

namespace IrisWeb.API.Controllers;

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

    /// <summary>Encerra as sessões de <c>userId</c>: o próprio usuário, ou qualquer um se for master.</summary>
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

    /// <summary>Confere matrícula e senha sem abrir sessão.</summary>
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
