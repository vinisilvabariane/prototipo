using FluentResults;
using IrisWeb.Application.Communication.Users.Requests;
using IrisWeb.Application.Communication.Users.Responses;
using IrisWeb.Application.Interfaces.Auth;
using IrisWeb.Application.Interfaces.Matrix;
using IrisWeb.Application.Interfaces.Users;
using IrisWeb.Application.UseCases.Collaborators;
using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.Users;

/// <summary>
/// Autocadastro anônimo. Só aceita matrícula que existe na base de colaboradores do Sync, e o
/// usuário nasce sem role nenhuma — quem dá acesso é um master.
/// </summary>
public sealed class RegisterUserUseCase(
    IUserRepository users,
    IDepartmentRepository departments,
    IPasswordHasher passwordHasher,
    ISyncMatrixClient sync) : IRegisterUserUseCase
{
    private const ulong SelfRegistered = 0;

    public async Task<Result<UserResponse>> ExecuteAsync(RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var collaborator = await CollaboratorsMapping.LookupAsync(sync, request.Registration, cancellationToken);
        if (collaborator.IsFailed)
            return collaborator.ToResult();

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
            SelfRegistered);

        await users.AddAsync(user, cancellationToken);

        if (await users.SaveChangesAsync(cancellationToken) <= 0)
            return Result.Fail(UserErrors.NothingPersisted);

        return Result.Ok(UsersMapping.ToResponse(user));
    }
}
