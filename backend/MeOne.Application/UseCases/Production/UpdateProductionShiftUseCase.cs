using FluentResults;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces.Production;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Production;

public sealed class UpdateProductionShiftUseCase(IProductionShiftRepository shifts) : IUpdateProductionShiftUseCase
{
    public async Task<Result> ExecuteAsync(ulong id, SaveProductionShiftRequest request, CancellationToken cancellationToken)
    {
        var shift = await shifts.GetByIdAsync(id, cancellationToken);
        if (shift is null)
            return Result.Fail(new NotFoundError(ProductionErrors.ShiftNotFound));

        if (await shifts.NameExistsAsync(ProductionShift.NormalizeName(request.Name), id, cancellationToken))
            return Result.Fail(ProductionErrors.NameTaken(ProductionShift.NormalizeName(request.Name)));

        if (!shift.Update(request.Name, request.StartTime, request.EndTime, request.Active))
            return Result.Ok();

        shifts.Update(shift);

        return await shifts.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok()
            : Result.Fail(ProductionErrors.NothingPersisted);
    }
}
