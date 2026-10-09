using FluentResults;
using IrisWeb.Application.Errors;
using IrisWeb.Application.Interfaces.Departments;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.Departments;

public sealed class DeleteDepartmentUseCase(IDepartmentRepository departments) : IDeleteDepartmentUseCase
{
    public async Task<Result> ExecuteAsync(int id, CancellationToken cancellationToken)
    {
        var department = await departments.GetByIdAsync(id, cancellationToken);
        if (department is null)
            return Result.Fail(new NotFoundError(DepartmentErrors.NotFound));

        if (await departments.InUseAsync(id, cancellationToken))
            return Result.Fail(DepartmentErrors.InUse);

        departments.Delete(department);

        return await departments.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok()
            : Result.Fail(DepartmentErrors.NothingPersisted);
    }
}
