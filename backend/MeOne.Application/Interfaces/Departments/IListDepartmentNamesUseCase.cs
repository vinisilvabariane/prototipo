using FluentResults;

namespace MeOne.Application.Interfaces.Departments;

public interface IListDepartmentNamesUseCase
{
    Task<Result<IReadOnlyList<string>>> ExecuteAsync(CancellationToken cancellationToken);
}
