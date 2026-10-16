using MeOne.API.Auth;
using MeOne.API.Errors;
using MeOne.Application.Communication.Common.Responses;
using MeOne.Application.Communication.Roles.Responses;
using MeOne.Application.Communication.Users.Requests;
using MeOne.Application.Communication.Users.Responses;
using MeOne.Application.Interfaces.Users;
using MeOne.Domain.Rules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace MeOne.API.Controllers;

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
    IRequestPasswordResetUseCase requestPasswordReset,
    IConfirmPasswordResetUseCase confirmPasswordReset,
    IIssueTemporaryPasswordUseCase issueTemporaryPassword,
    IChangePasswordUseCase changePassword,
    IListUserRolesUseCase listUserRoles) : ControllerBase
{
    [HttpGet]
    [RequirePermission(Permissions.UsersRead)]
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
    [RequirePermission(Permissions.UsersRead)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetById(ulong id, CancellationToken cancellationToken)
    {
        var result = await getUser.ExecuteAsync(id, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Falha ao buscar usuário", result);
    }

    [HttpGet("get-by-registration/{registration}")]
    [RequirePermission(Permissions.UsersRead)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserResponse>> GetByRegistration(uint registration, CancellationToken cancellationToken)
    {
        var result = await getUserByRegistration.ExecuteAsync(registration, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Falha ao verificar usuário", result);
    }

    [HttpGet("roles/{userId:long}")]
    [RequirePermission(Permissions.UsersRead)]
    [ProducesResponseType(typeof(IReadOnlyList<RoleAssignmentResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RoleAssignmentResponse>>> Roles(
        ulong userId,
        CancellationToken cancellationToken)
    {
        var result = await listUserRoles.ExecuteAsync(userId, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ApiResults.Failure("Falha ao obter papéis", result);
    }

    [HttpPost]
    [RequirePermission(Permissions.UsersManage)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await createUser.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(GetById), new { id = result.Value.Id }, result.Value)
            : ApiResults.Failure("Falha ao criar usuário", result);
    }

    [HttpPut("{id:long}")]
    [RequirePermission(Permissions.UsersManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(ulong id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await updateUser.ExecuteAsync(id, request, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Falha ao atualizar usuário", result);
    }

    [HttpDelete("{id:long}")]
    [RequirePermission(Permissions.UsersManage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(ulong id, CancellationToken cancellationToken)
    {
        var result = await deleteUser.ExecuteAsync(id, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Erro ao desativar usuário", result);
    }

    [HttpPut("reset-password-manually")]
    [RequirePermission(Permissions.UsersResetPassword)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var result = await changePassword.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : ApiResults.Failure("Erro ao tentar atualizar a senha do usuário", result);
    }

    [HttpPost("{id:long}/temporary-password")]
    [RequirePermission(Permissions.UsersResetPassword)]
    [ProducesResponseType(typeof(TemporaryPasswordResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TemporaryPasswordResponse>> IssueTemporaryPassword(
        ulong id,
        CancellationToken cancellationToken)
    {
        var result = await issueTemporaryPassword.ExecuteAsync(id, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : ApiResults.Failure("Erro ao gerar a senha provisória", result);
    }

    [HttpPost("password-reset")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthPolicies.AnonymousRateLimit)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RequestPasswordReset(
        RequestPasswordResetRequest request,
        CancellationToken cancellationToken)
    {
        await requestPasswordReset.ExecuteAsync(request, cancellationToken);

        return Accepted();
    }

    [HttpPost("password-reset/confirm")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthPolicies.AnonymousRateLimit)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ConfirmPasswordReset(
        ConfirmPasswordResetRequest request,
        CancellationToken cancellationToken)
    {
        var result = await confirmPasswordReset.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess ? NoContent() : ApiResults.Failure("Não foi possível redefinir a senha", result);
    }

    [HttpPost("not-authenticated")]
    [AllowAnonymous]
    [EnableRateLimiting(AuthPolicies.AnonymousRateLimit)]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<UserResponse>> Register(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var result = await registerUser.ExecuteAsync(request, cancellationToken);

        return result.IsSuccess
            ? StatusCode(StatusCodes.Status201Created, result.Value)
            : ApiResults.Failure("Erro ao criar usuário", result);
    }
}
