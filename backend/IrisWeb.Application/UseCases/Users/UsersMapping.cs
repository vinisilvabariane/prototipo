using IrisWeb.Application.Communication.Users.Responses;
using IrisWeb.Application.Interfaces;
using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.Users;

internal static class UsersMapping
{
    /// <summary>Quem está mexendo, para <c>modified_by</c>. 0 = sem usuário logado (autocadastro).</summary>
    public static ulong ActorId(ICurrentActor currentActor) => currentActor.UserId ?? 0;

    public static UserResponse ToResponse(User user) => new(
        user.Id,
        user.Registration,
        user.Name,
        user.Email,
        user.Department?.Name ?? string.Empty);

    /// <summary>Matrícula e e-mail são únicos no banco, inclusive entre usuários desativados.</summary>
    public static async Task<string?> UniquenessViolationAsync(
        IUserRepository users,
        uint registration,
        string? normalizedEmail,
        ulong? exceptUserId,
        CancellationToken cancellationToken)
    {
        if (await users.RegistrationExistsAsync(registration, exceptUserId, cancellationToken))
            return UserErrors.RegistrationTaken;

        if (normalizedEmail is not null &&
            await users.EmailExistsAsync(normalizedEmail, exceptUserId, cancellationToken))
        {
            return UserErrors.EmailTaken;
        }

        return null;
    }

    public static Task<Department?> FindDepartmentAsync(
        IDepartmentRepository departments,
        string name,
        CancellationToken cancellationToken) =>
        departments.GetByNameAsync(Department.Normalize(name), cancellationToken);
}
