using FluentResults;
using MeOne.Application.Communication.Suppliers.Requests;
using MeOne.Application.Communication.Suppliers.Responses;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Suppliers;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Suppliers;

public sealed class AddSupplierAuditUseCase(
    ISupplierRepository suppliers,
    IUserRepository users,
    ICurrentActor currentActor) : IAddSupplierAuditUseCase
{
    public async Task<Result<SupplierAuditResponse>> ExecuteAsync(
        ulong supplierId,
        SaveAuditRequest request,
        CancellationToken cancellationToken)
    {
        var loaded = await SuppliersMapping.LoadEditableAsync(suppliers, currentActor, supplierId, cancellationToken);
        if (loaded.IsFailed)
            return loaded.ToResult<SupplierAuditResponse>();

        var (supplier, actorId) = loaded.Value;

        if (!await SuppliersMapping.IsActiveUserAsync(users, request.ResponsibleId, cancellationToken))
            return Result.Fail<SupplierAuditResponse>(new NotFoundError(SupplierErrors.ResponsibleNotFound));

        var responsibleName = (await users.GetByIdsIncludingInactiveAsync([request.ResponsibleId], cancellationToken)).FirstOrDefault()?.Name ?? string.Empty;

        var item = supplier.AddAudit(request.ResponsibleId, request.AuditedOn, request.ValidityMonths, request.Score, request.Status, request.ChecklistCode, request.ChecklistRevision);

        supplier.MarkUpdated(actorId, DateTime.UtcNow);
        suppliers.Update(supplier);

        return await suppliers.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok(SuppliersMapping.ToAudit(item, responsibleName, SuppliersMapping.Today()))
            : Result.Fail<SupplierAuditResponse>(SupplierErrors.NothingPersisted);
    }
}
