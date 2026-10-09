using FluentResults;

namespace IrisWeb.Application.Interfaces.Departments;

public interface IDeleteDepartmentUseCase
{
    Task<Result> ExecuteAsync(int id, CancellationToken cancellationToken);
}
