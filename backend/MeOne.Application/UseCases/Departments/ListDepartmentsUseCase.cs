using FluentResults;
using MeOne.Application.Communication.Departments.Responses;
using MeOne.Application.Interfaces.Departments;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Departments;

public sealed class ListDepartmentsUseCase(IDepartmentRepository departments) : IListDepartmentsUseCase
{
    public async Task<Result<IReadOnlyList<DepartmentResponse>>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var all = await departments.ListOrderedAsync(cancellationToken);

        return Result.Ok<IReadOnlyList<DepartmentResponse>>([.. all.Select(DepartmentsMapping.ToResponse)]);
    }
}
