using FluentResults;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces.OrgChart;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.OrgChart;

public sealed class DeleteOrgPositionUseCase(IOrgPositionRepository positions) : IDeleteOrgPositionUseCase
{
    public async Task<Result> ExecuteAsync(ulong id, CancellationToken cancellationToken)
    {
        var position = await positions.GetByIdAsync(id, cancellationToken);
        if (position is null)
            return Result.Fail(new NotFoundError(OrgChartErrors.NotFound));

        if (await positions.HasSubordinatesAsync(id, cancellationToken))
            return Result.Fail(OrgChartErrors.HasSubordinates);

        positions.Delete(position);

        return await positions.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok()
            : Result.Fail(OrgChartErrors.NothingPersisted);
    }
}
