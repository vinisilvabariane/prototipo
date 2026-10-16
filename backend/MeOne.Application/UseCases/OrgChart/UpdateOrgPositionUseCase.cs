using FluentResults;
using MeOne.Application.Communication.OrgChart.Requests;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces.OrgChart;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.OrgChart;

public sealed class UpdateOrgPositionUseCase(IOrgPositionRepository positions, IDepartmentRepository departments)
    : IUpdateOrgPositionUseCase
{
    public async Task<Result> ExecuteAsync(ulong id, UpdateOrgPositionRequest request, CancellationToken cancellationToken)
    {
        var position = await positions.GetByIdAsync(id, cancellationToken);
        if (position is null)
            return Result.Fail(new NotFoundError(OrgChartErrors.NotFound));

        var department = await departments.GetByIdAsync(request.DepartmentId, cancellationToken);
        if (department is null)
            return Result.Fail(new NotFoundError(DepartmentErrors.NotFound));

        if (!position.Update(request.Title, department.Id))
            return Result.Ok();

        positions.Update(position);

        return await positions.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok()
            : Result.Fail(OrgChartErrors.NothingPersisted);
    }
}
