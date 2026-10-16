using FluentResults;
using MeOne.Application.Communication.Suppliers.Requests;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Suppliers;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Suppliers;

public sealed class UpdateSupplierMaterialUseCase(
    ISupplierRepository suppliers,
    ICurrentActor currentActor) : IUpdateSupplierMaterialUseCase
{
    public async Task<Result> ExecuteAsync(
        ulong supplierId,
        ulong itemId,
        SaveSupplierMaterialRequest request,
        CancellationToken cancellationToken)
    {
        var loaded = await SuppliersMapping.LoadEditableAsync(suppliers, currentActor, supplierId, cancellationToken);
        if (loaded.IsFailed)
            return loaded.ToResult();

        var (supplier, actorId) = loaded.Value;

        var item = supplier.FindMaterial(itemId);
        if (item is null)
            return Result.Fail(new NotFoundError(SupplierErrors.MaterialNotFound));

        if (!item.Update(request.Sku, request.BusinessUnit, request.MaterialType, request.Criticality, request.HasSapSpecification))
            return Result.Ok();

        supplier.MarkUpdated(actorId, DateTime.UtcNow);
        suppliers.Update(supplier);

        return await suppliers.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok()
            : Result.Fail(SupplierErrors.NothingPersisted);
    }
}
