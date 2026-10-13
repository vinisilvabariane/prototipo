using FluentResults;
using MeOne.Application.Communication.Departments.Requests;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces.Departments;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Departments;

public sealed class UpdateDepartmentUseCase(IDepartmentRepository departments) : IUpdateDepartmentUseCase
{
    public async Task<Result> ExecuteAsync(int id, UpdateDepartmentRequest request, CancellationToken cancellationToken)
    {
        var department = await departments.GetByIdAsync(id, cancellationToken);
        if (department is null)
            return Result.Fail(new NotFoundError(DepartmentErrors.NotFound));

        var name = Department.Normalize(request.Name);

        if (await departments.NameExistsAsync(name, exceptDepartmentId: id, cancellationToken))
            return Result.Fail(DepartmentErrors.NameTaken(name));

        if (!department.Rename(name))
            return Result.Ok();

        departments.Update(department);

        return await departments.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok()
            : Result.Fail(DepartmentErrors.NothingPersisted);
    }
}
