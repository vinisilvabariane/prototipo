using FluentResults;
using MeOne.Application.Communication.Suppliers.Responses;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Suppliers;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Suppliers;

public sealed class GetSupplierUseCase(ISupplierRepository suppliers, IUserRepository users, ICurrentActor currentActor)
    : IGetSupplierUseCase
{
    public async Task<Result<SupplierDetailResponse>> ExecuteAsync(ulong id, CancellationToken cancellationToken)
    {
        var supplier = await suppliers.GetByIdAsync(id, cancellationToken);
        if (supplier is null)
            return Result.Fail<SupplierDetailResponse>(new NotFoundError(SupplierErrors.NotFound));

        var ids = supplier.SelfAssessments.Select(item => item.ResponsibleId)
            .Concat(supplier.Audits.Select(item => item.ResponsibleId))
            .Append(supplier.CreatedBy)
            .Append(supplier.UpdatedBy)
            .Distinct()
            .ToList();

        var names = (await users.GetByIdsIncludingInactiveAsync(ids, cancellationToken)).ToDictionary(user => user.Id, user => user.Name);

        string NameOf(ulong userId) => names.GetValueOrDefault(userId, string.Empty);

        var today = SuppliersMapping.Today();
        var profile = await currentActor.GetAccessProfileAsync(cancellationToken);

        return Result.Ok(new SupplierDetailResponse(
            supplier.Id,
            supplier.SapCode,
            supplier.Name,
            supplier.Branch,
            supplier.MultiBranch,
            supplier.Origin,
            supplier.TaxId,
            supplier.CorporateName,
            supplier.Criticality,
            supplier.ZipCode,
            supplier.District,
            supplier.Street,
            supplier.Number,
            supplier.ContactName,
            supplier.Phone,
            supplier.Email,
            NameOf(supplier.CreatedBy),
            supplier.CreatedAt,
            NameOf(supplier.UpdatedBy),
            supplier.UpdatedAt,
            SuppliersMapping.ToQualification(supplier.CurrentQualification, today),
            [.. supplier.Tools.Select(SuppliersMapping.ToTool)],
            [.. supplier.Materials.Select(SuppliersMapping.ToMaterial)],
            [.. supplier.SelfAssessments
                .OrderByDescending(item => item.AssessedOn)
                .Select(item => SuppliersMapping.ToSelfAssessment(item, NameOf(item.ResponsibleId), today))],
            [.. supplier.Audits
                .OrderByDescending(item => item.AuditedOn)
                .Select(item => SuppliersMapping.ToAudit(item, NameOf(item.ResponsibleId), today))],
            [.. supplier.Documents.OrderByDescending(item => item.UploadedAt).Select(item => SuppliersMapping.ToDocument(item, today))],
            profile is not null && SuppliersMapping.CanEdit(profile, supplier)));
    }
}
