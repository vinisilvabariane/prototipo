using FluentResults;
using MeOne.Application.Communication.Suppliers.Requests;
using MeOne.Application.Communication.Suppliers.Responses;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Suppliers;
using MeOne.Application.UseCases.Auth;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Suppliers;

public sealed class CreateSupplierUseCase(ISupplierRepository suppliers, ICurrentActor currentActor) : ICreateSupplierUseCase
{
    public async Task<Result<SupplierSummaryResponse>> ExecuteAsync(SaveSupplierRequest request, CancellationToken cancellationToken)
    {
        var actor = await AccessGuard.ActorAsync(currentActor, cancellationToken);
        if (actor.IsFailed)
            return actor.ToResult<SupplierSummaryResponse>();

        var code = Supplier.NormalizeSapCode(request.SapCode);
        if (await suppliers.SapCodeExistsAsync(code, request.MultiBranch, null, cancellationToken))
            return Result.Fail<SupplierSummaryResponse>(SupplierErrors.SapCodeTaken(code, request.MultiBranch.ToString()));

        var supplier = Supplier.Create(
            request.SapCode,
            request.Name,
            request.Branch,
            request.MultiBranch,
            request.Origin,
            request.TaxId,
            request.CorporateName,
            request.Criticality,
            request.ZipCode,
            request.District,
            request.Street,
            request.Number,
            request.ContactName,
            request.Phone,
            request.Email,
            actor.Value.UserId,
            DateTime.UtcNow);

        await suppliers.AddAsync(supplier, cancellationToken);

        return await suppliers.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok(SuppliersMapping.ToSummary(supplier, SuppliersMapping.Today()))
            : Result.Fail<SupplierSummaryResponse>(SupplierErrors.NothingPersisted);
    }
}
