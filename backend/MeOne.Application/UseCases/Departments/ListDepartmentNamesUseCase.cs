using FluentResults;
using MeOne.Application.Interfaces.Departments;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Departments;

public sealed class ListDepartmentNamesUseCase(IDepartmentRepository departments) : IListDepartmentNamesUseCase
{
    public async Task<Result<IReadOnlyList<string>>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var all = await departments.ListOrderedAsync(cancellationToken);

        return Result.Ok<IReadOnlyList<string>>([.. all.Select(department => department.Name)]);
    }
}
