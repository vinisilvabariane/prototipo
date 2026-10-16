using FluentResults;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Communication.Production.Responses;
using MeOne.Application.Interfaces.Production;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Production;

public sealed class CreateProductionLineUseCase(IProductionLineRepository lines) : ICreateProductionLineUseCase
{
    public async Task<Result<ProductionLineResponse>> ExecuteAsync(SaveProductionLineRequest request, CancellationToken cancellationToken)
    {
        if (await lines.NameExistsAsync(ProductionLine.NormalizeName(request.Name), null, cancellationToken))
            return Result.Fail<ProductionLineResponse>(ProductionErrors.NameTaken(ProductionLine.NormalizeName(request.Name)));

        var created = ProductionLine.Create(request.Name, DateTime.UtcNow);
        await lines.AddAsync(created, cancellationToken);

        return await lines.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok(ProductionMapping.ToResponse(created))
            : Result.Fail<ProductionLineResponse>(ProductionErrors.NothingPersisted);
    }
}
