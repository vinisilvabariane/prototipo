using FluentResults;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces.Production;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Production;

public sealed class UpdateProductionProductUseCase(IProductionProductRepository products) : IUpdateProductionProductUseCase
{
    public async Task<Result> ExecuteAsync(ulong id, SaveProductionProductRequest request, CancellationToken cancellationToken)
    {
        var product = await products.GetByIdAsync(id, cancellationToken);
        if (product is null)
            return Result.Fail(new NotFoundError(ProductionErrors.ProductNotFound));

        if (await products.CodeExistsAsync(ProductionProduct.NormalizeCode(request.Code), id, cancellationToken))
            return Result.Fail(ProductionErrors.CodeTaken(ProductionProduct.NormalizeCode(request.Code)));

        if (!product.Update(request.Code, request.Name, request.Active))
            return Result.Ok();

        products.Update(product);

        return await products.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok()
            : Result.Fail(ProductionErrors.NothingPersisted);
    }
}
