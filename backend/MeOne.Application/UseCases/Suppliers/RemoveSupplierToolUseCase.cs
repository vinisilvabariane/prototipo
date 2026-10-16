using FluentResults;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Suppliers;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Suppliers;

public sealed class RemoveSupplierToolUseCase(ISupplierRepository suppliers, ICurrentActor currentActor)
    : IRemoveSupplierToolUseCase
{
    public async Task<Result> ExecuteAsync(ulong supplierId, ulong itemId, CancellationToken cancellationToken)
    {
        var loaded = await SuppliersMapping.LoadEditableAsync(suppliers, currentActor, supplierId, cancellationToken);
        if (loaded.IsFailed)
            return loaded.ToResult();

        var (supplier, actorId) = loaded.Value;

        var item = supplier.FindTool(itemId);
        if (item is null)
            return Result.Fail(new NotFoundError(SupplierErrors.ToolNotFound));

        supplier.RemoveTool(item);

        supplier.MarkUpdated(actorId, DateTime.UtcNow);
        suppliers.Update(supplier);

        return await suppliers.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok()
            : Result.Fail(SupplierErrors.NothingPersisted);
    }
}
