using FluentResults;
using IrisWeb.Application.Interfaces.Departments;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.Departments;

/// <summary>Só os nomes: é o formato que o SYNC/Q-Zero consome. Não mudar para objeto.</summary>
public sealed class ListDepartmentNamesUseCase(IDepartmentRepository departments) : IListDepartmentNamesUseCase
{
    public async Task<Result<IReadOnlyList<string>>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var all = await departments.ListOrderedAsync(cancellationToken);

        return Result.Ok<IReadOnlyList<string>>([.. all.Select(department => department.Name)]);
    }
}
