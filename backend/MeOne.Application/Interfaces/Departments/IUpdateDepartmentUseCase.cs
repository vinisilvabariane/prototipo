using FluentResults;
using MeOne.Application.Communication.Departments.Requests;

namespace MeOne.Application.Interfaces.Departments;

public interface IUpdateDepartmentUseCase
{
    Task<Result> ExecuteAsync(int id, UpdateDepartmentRequest request, CancellationToken cancellationToken);
}
