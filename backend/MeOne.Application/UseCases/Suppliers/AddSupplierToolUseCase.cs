using FluentResults;
using MeOne.Application.Communication.Suppliers.Requests;
using MeOne.Application.Communication.Suppliers.Responses;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Suppliers;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Suppliers;

public sealed class AddSupplierToolUseCase(
    ISupplierRepository suppliers,
    ICurrentActor currentActor) : IAddSupplierToolUseCase
{
    public async Task<Result<SupplierToolResponse>> ExecuteAsync(
        ulong supplierId,
        SaveSupplierToolRequest request,
        CancellationToken cancellationToken)
    {
        var loaded = await SuppliersMapping.LoadEditableAsync(suppliers, currentActor, supplierId, cancellationToken);
        if (loaded.IsFailed)
            return loaded.ToResult<SupplierToolResponse>();

        var (supplier, actorId) = loaded.Value;

        var item = supplier.AddTool(request.Name, request.AssetTag);

        supplier.MarkUpdated(actorId, DateTime.UtcNow);
        suppliers.Update(supplier);

        return await suppliers.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok(SuppliersMapping.ToTool(item))
            : Result.Fail<SupplierToolResponse>(SupplierErrors.NothingPersisted);
    }
}
