using FluentResults;
using IrisWeb.Application.Communication.Departments.Requests;
using IrisWeb.Application.Communication.Departments.Responses;
using IrisWeb.Application.Interfaces.Departments;
using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.Departments;

public sealed class CreateDepartmentUseCase(IDepartmentRepository departments) : ICreateDepartmentUseCase
{
    public async Task<Result<DepartmentResponse>> ExecuteAsync(
        CreateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        var department = Department.Create(request.Name);

        if (await departments.NameExistsAsync(department.Name, exceptDepartmentId: null, cancellationToken))
            return Result.Fail(DepartmentErrors.NameTaken(department.Name));

        await departments.AddAsync(department, cancellationToken);

        if (await departments.SaveChangesAsync(cancellationToken) <= 0)
            return Result.Fail(DepartmentErrors.NothingPersisted);

        return Result.Ok(DepartmentsMapping.ToResponse(department));
    }
}
