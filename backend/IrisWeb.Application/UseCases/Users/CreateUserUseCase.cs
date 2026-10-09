using FluentResults;
using IrisWeb.Application.Communication.Users.Requests;
using IrisWeb.Application.Communication.Users.Responses;
using IrisWeb.Application.Interfaces;
using IrisWeb.Application.Interfaces.Auth;
using IrisWeb.Application.Interfaces.Users;
using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.Users;

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
