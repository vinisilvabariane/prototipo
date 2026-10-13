using FluentResults;
using MeOne.Application.Communication.Departments.Requests;
using MeOne.Application.Communication.Departments.Responses;

namespace MeOne.Application.Interfaces.Departments;

public interface ICreateDepartmentUseCase
{
    Task<Result<DepartmentResponse>> ExecuteAsync(CreateDepartmentRequest request, CancellationToken cancellationToken);
}
