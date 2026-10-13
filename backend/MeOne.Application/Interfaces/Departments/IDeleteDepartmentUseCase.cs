using FluentResults;

namespace MeOne.Application.Interfaces.Departments;

public interface IDeleteDepartmentUseCase
{
    Task<Result> ExecuteAsync(int id, CancellationToken cancellationToken);
}
