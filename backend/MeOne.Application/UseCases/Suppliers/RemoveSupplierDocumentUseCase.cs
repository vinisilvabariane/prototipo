using FluentResults;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Suppliers;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Suppliers;

public sealed class RemoveSupplierDocumentUseCase(
    ISupplierRepository suppliers,
    ISupplierFileStorage storage,
    ICurrentActor currentActor) : IRemoveSupplierDocumentUseCase
{
    public async Task<Result> ExecuteAsync(ulong supplierId, ulong documentId, CancellationToken cancellationToken)
    {
        var loaded = await SuppliersMapping.LoadEditableAsync(suppliers, currentActor, supplierId, cancellationToken);
        if (loaded.IsFailed)
            return loaded.ToResult();

        var (supplier, actorId) = loaded.Value;

        var document = supplier.DetachDocument(documentId);
        if (document is null)
            return Result.Fail(new NotFoundError(SupplierErrors.DocumentNotFound));

        supplier.MarkUpdated(actorId, DateTime.UtcNow);
        suppliers.Update(supplier);

        if (await suppliers.SaveChangesAsync(cancellationToken) <= 0)
            return Result.Fail(SupplierErrors.NothingPersisted);

        await storage.DeleteAsync(document.StoredName, CancellationToken.None);
        return Result.Ok();
    }
}
