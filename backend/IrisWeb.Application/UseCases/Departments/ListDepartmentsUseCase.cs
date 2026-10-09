using FluentResults;
using IrisWeb.Application.Communication.Departments.Responses;
using IrisWeb.Application.Interfaces.Departments;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.Departments;

public sealed class ListDepartmentsUseCase(IDepartmentRepository departments) : IListDepartmentsUseCase
{
    public async Task<Result<IReadOnlyList<DepartmentResponse>>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var all = await departments.ListOrderedAsync(cancellationToken);

        return Result.Ok<IReadOnlyList<DepartmentResponse>>([.. all.Select(DepartmentsMapping.ToResponse)]);
    }
}
