using FluentResults;
using MeOne.Application.Communication.Suppliers.Requests;
using MeOne.Application.Communication.Suppliers.Responses;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Suppliers;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Suppliers;

public sealed class AddSupplierMaterialUseCase(
    ISupplierRepository suppliers,
    ICurrentActor currentActor) : IAddSupplierMaterialUseCase
{
    public async Task<Result<SupplierMaterialResponse>> ExecuteAsync(
        ulong supplierId,
        SaveSupplierMaterialRequest request,
        CancellationToken cancellationToken)
    {
        var loaded = await SuppliersMapping.LoadEditableAsync(suppliers, currentActor, supplierId, cancellationToken);
        if (loaded.IsFailed)
            return loaded.ToResult<SupplierMaterialResponse>();

        var (supplier, actorId) = loaded.Value;

        var item = supplier.AddMaterial(request.Sku, request.BusinessUnit, request.MaterialType, request.Criticality, request.HasSapSpecification);

        supplier.MarkUpdated(actorId, DateTime.UtcNow);
        suppliers.Update(supplier);

        return await suppliers.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok(SuppliersMapping.ToMaterial(item))
            : Result.Fail<SupplierMaterialResponse>(SupplierErrors.NothingPersisted);
    }
}
