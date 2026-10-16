using FluentResults;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Communication.Production.Responses;
using MeOne.Application.Interfaces.Production;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Production;

public sealed class CreateProductionShiftUseCase(IProductionShiftRepository shifts) : ICreateProductionShiftUseCase
{
    public async Task<Result<ProductionShiftResponse>> ExecuteAsync(SaveProductionShiftRequest request, CancellationToken cancellationToken)
    {
        if (await shifts.NameExistsAsync(ProductionShift.NormalizeName(request.Name), null, cancellationToken))
            return Result.Fail<ProductionShiftResponse>(ProductionErrors.NameTaken(ProductionShift.NormalizeName(request.Name)));

        var created = ProductionShift.Create(request.Name, request.StartTime, request.EndTime);
        await shifts.AddAsync(created, cancellationToken);

        return await shifts.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok(ProductionMapping.ToResponse(created))
            : Result.Fail<ProductionShiftResponse>(ProductionErrors.NothingPersisted);
    }
}
