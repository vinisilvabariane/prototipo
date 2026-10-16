using FluentResults;
using MeOne.Application.Communication.Suppliers.Responses;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.UseCases.Auth;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.Suppliers;

internal sealed record EditableSupplier(Supplier Supplier, ulong ActorId);

internal static class SuppliersMapping
{
    private const int BrasiliaOffsetHours = -3;

    public static DateOnly Today() => DateOnly.FromDateTime(DateTime.UtcNow.AddHours(BrasiliaOffsetHours));

    public static bool CanEdit(AccessProfile actor, Supplier supplier) =>
        supplier.CreatedBy == actor.UserId || actor.Allows(Permissions.SuppliersManage);

    public static async Task<Result<EditableSupplier>> LoadEditableAsync(
        ISupplierRepository suppliers,
        ICurrentActor currentActor,
        ulong id,
        CancellationToken cancellationToken)
    {
        var actor = await AccessGuard.ActorAsync(currentActor, cancellationToken);
        if (actor.IsFailed)
            return actor.ToResult<EditableSupplier>();

        var supplier = await suppliers.GetByIdAsync(id, cancellationToken);
        if (supplier is null)
            return Result.Fail<EditableSupplier>(new NotFoundError(SupplierErrors.NotFound));

        return CanEdit(actor.Value, supplier)
            ? Result.Ok(new EditableSupplier(supplier, actor.Value.UserId))
            : Result.Fail<EditableSupplier>(new ForbiddenError(SupplierErrors.OnlyOwnerOrManagerEdits));
    }

    public static async Task<bool> IsActiveUserAsync(IUserRepository users, ulong id, CancellationToken cancellationToken) =>
        (await users.GetActiveByIdsAsync([id], cancellationToken)).Count > 0;

    public static SupplierQualificationResponse? ToQualification(SupplierQualification? qualification, DateOnly today) =>
        qualification is null
            ? null
            : new(
                qualification.Kind,
                qualification.Status,
                qualification.Score,
                qualification.PerformedOn,
                qualification.ExpiresOn,
                qualification.IsExpired(today),
                qualification.AuditPending);

    public static SupplierSummaryResponse ToSummary(Supplier supplier, DateOnly today) => new(
        supplier.Id,
        supplier.SapCode,
        supplier.Name,
        supplier.Branch,
        supplier.MultiBranch,
        supplier.Origin,
        supplier.Criticality,
        ToQualification(supplier.CurrentQualification, today));

    public static SupplierToolResponse ToTool(SupplierTool tool) => new(tool.Id, tool.Name, tool.AssetTag);

    public static SupplierMaterialResponse ToMaterial(SupplierMaterial material) => new(
        material.Id,
        material.Sku,
        material.BusinessUnit,
        material.MaterialType,
        material.Criticality,
        material.HasSapSpecification);

    public static SupplierSelfAssessmentResponse ToSelfAssessment(
        SupplierSelfAssessment assessment,
        string responsibleName,
        DateOnly today) => new(
        assessment.Id,
        assessment.ResponsibleId,
        responsibleName,
        assessment.AssessedOn,
        assessment.ExpiresOn,
        assessment.ExpiresOn < today,
        assessment.Score,
        assessment.Status,
        assessment.EditableLink,
        assessment.DocumentCode,
        assessment.Revision,
        assessment.NeedsOnSiteAudit);

    public static SupplierAuditResponse ToAudit(SupplierAudit audit, string responsibleName, DateOnly today) => new(
        audit.Id,
        audit.ResponsibleId,
        responsibleName,
        audit.AuditedOn,
        audit.ValidityMonths,
        audit.ExpiresOn,
        audit.ExpiresOn < today,
        audit.Score,
        audit.Status,
        audit.ChecklistCode,
        audit.ChecklistRevision);

    public static SupplierDocumentResponse ToDocument(SupplierDocument document, DateOnly today) => new(
        document.Id,
        document.Kind,
        document.OwnerId,
        document.OriginalName,
        document.ContentType,
        document.Size,
        document.Version,
        document.ExpiresOn,
        document.ExpiresOn is { } expiresOn && expiresOn < today,
        document.UploadedAt);
}
