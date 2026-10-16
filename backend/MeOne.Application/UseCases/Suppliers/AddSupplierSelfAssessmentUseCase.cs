using FluentResults;
using MeOne.Application.Communication.Suppliers.Requests;
using MeOne.Application.Communication.Suppliers.Responses;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Suppliers;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Suppliers;

public sealed class AddSupplierSelfAssessmentUseCase(
    ISupplierRepository suppliers,
    IUserRepository users,
    ICurrentActor currentActor) : IAddSupplierSelfAssessmentUseCase
{
    public async Task<Result<SupplierSelfAssessmentResponse>> ExecuteAsync(
        ulong supplierId,
        SaveSelfAssessmentRequest request,
        CancellationToken cancellationToken)
    {
        var loaded = await SuppliersMapping.LoadEditableAsync(suppliers, currentActor, supplierId, cancellationToken);
        if (loaded.IsFailed)
            return loaded.ToResult<SupplierSelfAssessmentResponse>();

        var (supplier, actorId) = loaded.Value;

        if (!await SuppliersMapping.IsActiveUserAsync(users, request.ResponsibleId, cancellationToken))
            return Result.Fail<SupplierSelfAssessmentResponse>(new NotFoundError(SupplierErrors.ResponsibleNotFound));

        var responsibleName = (await users.GetByIdsIncludingInactiveAsync([request.ResponsibleId], cancellationToken)).FirstOrDefault()?.Name ?? string.Empty;

        var item = supplier.AddSelfAssessment(request.ResponsibleId, request.AssessedOn, request.Score, request.Status, request.EditableLink, request.DocumentCode, request.Revision, request.NeedsOnSiteAudit);

        supplier.MarkUpdated(actorId, DateTime.UtcNow);
        suppliers.Update(supplier);

        return await suppliers.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok(SuppliersMapping.ToSelfAssessment(item, responsibleName, SuppliersMapping.Today()))
            : Result.Fail<SupplierSelfAssessmentResponse>(SupplierErrors.NothingPersisted);
    }
}
