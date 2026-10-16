using FluentResults;
using MeOne.Application.Communication.Suppliers.Requests;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Suppliers;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Suppliers;

public sealed class UpdateSupplierAuditUseCase(
    ISupplierRepository suppliers,
    IUserRepository users,
    ICurrentActor currentActor) : IUpdateSupplierAuditUseCase
{
    public async Task<Result> ExecuteAsync(
        ulong supplierId,
        ulong itemId,
        SaveAuditRequest request,
        CancellationToken cancellationToken)
    {
        var loaded = await SuppliersMapping.LoadEditableAsync(suppliers, currentActor, supplierId, cancellationToken);
        if (loaded.IsFailed)
            return loaded.ToResult();

        var (supplier, actorId) = loaded.Value;

        var item = supplier.FindAudit(itemId);
        if (item is null)
            return Result.Fail(new NotFoundError(SupplierErrors.AuditNotFound));

        if (item.ResponsibleId != request.ResponsibleId && !await SuppliersMapping.IsActiveUserAsync(users, request.ResponsibleId, cancellationToken))
            return Result.Fail(new NotFoundError(SupplierErrors.ResponsibleNotFound));

        if (!item.Update(request.ResponsibleId, request.AuditedOn, request.ValidityMonths, request.Score, request.Status, request.ChecklistCode, request.ChecklistRevision))
            return Result.Ok();

        supplier.MarkUpdated(actorId, DateTime.UtcNow);
        suppliers.Update(supplier);

        return await suppliers.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok()
            : Result.Fail(SupplierErrors.NothingPersisted);
    }
}
