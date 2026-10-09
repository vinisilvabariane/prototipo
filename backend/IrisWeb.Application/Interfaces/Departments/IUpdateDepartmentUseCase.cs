using FluentResults;
using IrisWeb.Application.Communication.Departments.Requests;

namespace IrisWeb.Application.Interfaces.Departments;

public interface IUpdateDepartmentUseCase
{
    Task<Result> ExecuteAsync(int id, UpdateDepartmentRequest request, CancellationToken cancellationToken);
}
