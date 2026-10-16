using FluentResults;
using MeOne.Application.Communication.Suppliers.Dtos;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces.Suppliers;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Suppliers;

public sealed class GetSupplierDocumentUseCase(ISupplierRepository suppliers, ISupplierFileStorage storage)
    : IGetSupplierDocumentUseCase
{
    public async Task<Result<SupplierFileDownload>> ExecuteAsync(
        ulong supplierId,
        ulong documentId,
        CancellationToken cancellationToken)
    {
        var supplier = await suppliers.GetByIdAsync(supplierId, cancellationToken);
        if (supplier is null)
            return Result.Fail<SupplierFileDownload>(new NotFoundError(SupplierErrors.NotFound));

        var document = supplier.FindDocument(documentId);
        if (document is null)
            return Result.Fail<SupplierFileDownload>(new NotFoundError(SupplierErrors.DocumentNotFound));

        var content = await storage.OpenReadAsync(document.StoredName, cancellationToken);
        if (content is null)
            return Result.Fail<SupplierFileDownload>(new NotFoundError(SupplierErrors.FileNotFound));

        return Result.Ok(new SupplierFileDownload(content, document.OriginalName, document.ContentType));
    }
}
