using IrisWeb.API.Auth;
using IrisWeb.API.Errors;
using IrisWeb.Application.Communication.Common.Responses;
using IrisWeb.Application.Communication.Roles.Responses;
using IrisWeb.Application.Communication.Users.Requests;
using IrisWeb.Application.Communication.Users.Responses;
using IrisWeb.Application.Interfaces.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace IrisWeb.API.Controllers;

/// <summary>
/// Leitura para qualquer usuário logado (sistemas externos consomem a listagem e as roles com
/// token comum). Escrita só master. As rotas anônimas são as da tela de login: esqueci a senha,
/// autocadastro e a consulta por matrícula.
/// </summary>
[ApiController]
[Route("api/v1/users")]
public class UsersController(
    IListUsersUseCase listUsers,
    IGetUserUseCase getUser,
    IGetUserByRegistrationUseCase getUserByRegistration,
    ICreateUserUseCase createUser,
    IRegisterUserUseCase registerUser,
    IUpdateUserUseCase updateUser,
    IDeleteUserUseCase deleteUser,
    IResetPasswordUseCase resetPassword,
    IChangePasswordUseCase changePassword,
    IListUserRolesUseCase listUserRoles) : ControllerBase
{
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(PagedResponse<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<UserResponse>>> List(
        [FromQuery] UsersQuery query,
        CancellationToken cancellationToken)
    {
        var result = await listUsers.ExecuteAsync(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Erro ao buscar usuários", result);
    }

    [HttpGet("{id:long}")]
    [Authorize]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetById(ulong id, CancellationToken cancellationToken)
    {
        var result = await getUser.ExecuteAsync(id, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Falha ao buscar usuário", result);
    }

    [HttpPost]
    [Authorize(Policy = AuthPolicies.MasterOnly)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserResponse>> Create(
        CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await createUser.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : ApiResults.Failure("Falha ao criar usuário", result);
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = AuthPolicies.MasterOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        ulong id,
        UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await updateUser.ExecuteAsync(id, request, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao atualizar usuário", result);
    }

    /// <summary>Desativa (soft delete). O corpo <c>{ deletedBy }</c> do Access Control é ignorado: vale o token.</summary>
    [HttpDelete("{id:long}")]
    [Authorize(Policy = AuthPolicies.MasterOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(ulong id, CancellationToken cancellationToken)
    {
        var result = await deleteUser.ExecuteAsync(id, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Erro ao desativar usuário", result);
    }

    [HttpGet("roles/{userId:long}")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<RoleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RoleResponse>>> Roles(
        ulong userId,
        CancellationToken cancellationToken)
    {
        var result = await listUserRoles.ExecuteAsync(userId, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Falha ao obter roles", result);
    }

    [HttpPut("reset-password-manually")]
    [Authorize(Policy = AuthPolicies.MasterOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await changePassword.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : ApiResults.Failure("Erro ao tentar atualizar a senha do usuário", result);
    }

    /// <summary>
    /// Esqueci a senha. Anônimo recebe a senha só por e-mail; master recebe também na resposta.
    /// </summary>
    [HttpPost("reset-password/{registration}")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthPolicies.AnonymousRateLimit)]
    [ProducesResponseType(typeof(ResetPasswordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ResetPasswordResponse>> ResetPassword(
        uint registration,
        CancellationToken cancellationToken)
    {
        var result = await resetPassword.ExecuteAsync(registration, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : ApiResults.Failure("Erro ao tentar resetar a senha do usuário", result);
    }

    [HttpGet("get-by-registration/{registration}")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthPolicies.AnonymousRateLimit)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserResponse>> GetByRegistration(
        uint registration,
        CancellationToken cancellationToken)
    {
        var result = await getUserByRegistration.ExecuteAsync(registration, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Falha ao verificar usuário", result);
    }

    /// <summary>Autocadastro pela tela pública; o usuário nasce sem role.</summary>
    [HttpPost("not-authenticated")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthPolicies.AnonymousRateLimit)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<UserResponse>> Register(
        RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        var result = await registerUser.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : ApiResults.Failure("Erro ao criar usuário", result);
    }
}
