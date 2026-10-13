using MeOne.Application.Communication.Users.Responses;
using MeOne.Application.Interfaces;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Users;

internal static class UsersMapping
{
    public static ulong ActorId(ICurrentActor currentActor) => currentActor.UserId ?? 0;

    public static UserResponse ToResponse(User user) => new(
        user.Id,
        user.Registration,
        user.Name,
        user.Email,
        user.Department?.Name ?? string.Empty);

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
