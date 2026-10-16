using FluentResults;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Communication.Production.Responses;
using MeOne.Application.Interfaces.Production;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Production;

public sealed class CreateProductionProductUseCase(IProductionProductRepository products) : ICreateProductionProductUseCase
{
    public async Task<Result<ProductionProductResponse>> ExecuteAsync(SaveProductionProductRequest request, CancellationToken cancellationToken)
    {
        if (await products.CodeExistsAsync(ProductionProduct.NormalizeCode(request.Code), null, cancellationToken))
            return Result.Fail<ProductionProductResponse>(ProductionErrors.CodeTaken(ProductionProduct.NormalizeCode(request.Code)));

        var created = ProductionProduct.Create(request.Code, request.Name);
        await products.AddAsync(created, cancellationToken);

        return await products.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok(ProductionMapping.ToResponse(created))
            : Result.Fail<ProductionProductResponse>(ProductionErrors.NothingPersisted);
    }
}
