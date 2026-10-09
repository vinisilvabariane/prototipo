using FluentResults;

namespace IrisWeb.Application.Interfaces.Departments;

public interface IListDepartmentNamesUseCase
{
    Task<Result<IReadOnlyList<string>>> ExecuteAsync(CancellationToken cancellationToken);
}
