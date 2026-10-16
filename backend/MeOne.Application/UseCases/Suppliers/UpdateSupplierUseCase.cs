using FluentResults;
using MeOne.Application.Communication.Suppliers.Requests;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Suppliers;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Suppliers;

public sealed class UpdateSupplierUseCase(ISupplierRepository suppliers, ICurrentActor currentActor) : IUpdateSupplierUseCase
{
    public async Task<Result> ExecuteAsync(ulong id, SaveSupplierRequest request, CancellationToken cancellationToken)
    {
        var loaded = await SuppliersMapping.LoadEditableAsync(suppliers, currentActor, id, cancellationToken);
        if (loaded.IsFailed)
            return loaded.ToResult();

        var (supplier, actorId) = loaded.Value;

        var code = Supplier.NormalizeSapCode(request.SapCode);
        if (await suppliers.SapCodeExistsAsync(code, request.MultiBranch, id, cancellationToken))
            return Result.Fail(SupplierErrors.SapCodeTaken(code, request.MultiBranch.ToString()));

        var changed = supplier.Update(
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
            actorId,
            DateTime.UtcNow);

        if (!changed)
            return Result.Ok();

        suppliers.Update(supplier);

        return await suppliers.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok()
            : Result.Fail(SupplierErrors.NothingPersisted);
    }
}
