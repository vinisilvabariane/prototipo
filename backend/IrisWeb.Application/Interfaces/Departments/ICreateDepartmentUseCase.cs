using FluentResults;
using IrisWeb.Application.Communication.Departments.Requests;
using IrisWeb.Application.Communication.Departments.Responses;

namespace IrisWeb.Application.Interfaces.Departments;

public interface ICreateDepartmentUseCase
{
    Task<Result<DepartmentResponse>> ExecuteAsync(CreateDepartmentRequest request, CancellationToken cancellationToken);
}
