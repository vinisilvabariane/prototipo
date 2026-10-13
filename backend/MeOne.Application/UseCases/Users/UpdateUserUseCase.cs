using FluentResults;
using MeOne.Application.Communication.Users.Requests;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Users;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Users;

public sealed class UpdateUserUseCase(
    IUserRepository users,
    IDepartmentRepository departments,
    ICurrentActor currentActor) : IUpdateUserUseCase
{
    public async Task<Result> ExecuteAsync(ulong id, UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await users.GetByIdAsync(id, cancellationToken);
        if (user is null)
            return Result.Fail(new NotFoundError(UserErrors.NotFound));

        var department = await UsersMapping.FindDepartmentAsync(departments, request.Department, cancellationToken);
        if (department is null)
            return Result.Fail(UserErrors.DepartmentInvalid);

        var email = User.NormalizeEmail(request.Email);

        var conflict = await UsersMapping.UniquenessViolationAsync(
            users, request.Registration, email, exceptUserId: id, cancellationToken);
        if (conflict is not null)
            return Result.Fail(conflict);

        user.Update(request.Registration, request.Name, email, department, UsersMapping.ActorId(currentActor));
        users.Update(user);

        return await users.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok()
            : Result.Fail(UserErrors.NothingPersisted);
    }
}
