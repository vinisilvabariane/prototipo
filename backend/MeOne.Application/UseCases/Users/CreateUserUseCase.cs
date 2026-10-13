using FluentResults;
using MeOne.Application.Communication.Users.Requests;
using MeOne.Application.Communication.Users.Responses;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Auth;
using MeOne.Application.Interfaces.Users;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Users;

public sealed class CreateUserUseCase(
    IUserRepository users,
    IDepartmentRepository departments,
    IPasswordHasher passwordHasher,
    ICurrentActor currentActor) : ICreateUserUseCase
{
    public async Task<Result<UserResponse>> ExecuteAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var department = await UsersMapping.FindDepartmentAsync(departments, request.Department, cancellationToken);
        if (department is null)
            return Result.Fail(UserErrors.DepartmentInvalid);

        var email = User.NormalizeEmail(request.Email);

        var conflict = await UsersMapping.UniquenessViolationAsync(
            users, request.Registration, email, exceptUserId: null, cancellationToken);
        if (conflict is not null)
            return Result.Fail(conflict);

        var user = User.Create(
            request.Registration,
            request.Name,
            email,
            passwordHasher.Hash(request.Password),
            department,
            UsersMapping.ActorId(currentActor));

        await users.AddAsync(user, cancellationToken);

        if (await users.SaveChangesAsync(cancellationToken) <= 0)
            return Result.Fail(UserErrors.NothingPersisted);

        return Result.Ok(UsersMapping.ToResponse(user));
    }
}
