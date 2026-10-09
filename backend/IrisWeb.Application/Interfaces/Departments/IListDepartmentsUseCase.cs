using FluentResults;
using IrisWeb.Application.Communication.Departments.Responses;

namespace IrisWeb.Application.Interfaces.Departments;

public interface IListDepartmentsUseCase
{
    Task<Result<IReadOnlyList<DepartmentResponse>>> ExecuteAsync(CancellationToken cancellationToken);
}
