using FluentResults;
using MeOne.Application.Communication.Departments.Responses;

namespace MeOne.Application.Interfaces.Departments;

public interface IListDepartmentsUseCase
{
    Task<Result<IReadOnlyList<DepartmentResponse>>> ExecuteAsync(CancellationToken cancellationToken);
}
