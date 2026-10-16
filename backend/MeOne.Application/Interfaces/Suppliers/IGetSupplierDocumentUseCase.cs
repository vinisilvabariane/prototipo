using FluentResults;
using MeOne.Application.Communication.Suppliers.Dtos;

namespace MeOne.Application.Interfaces.Suppliers;

public interface IGetSupplierDocumentUseCase
{
    Task<Result<SupplierFileDownload>> ExecuteAsync(ulong supplierId, ulong documentId, CancellationToken cancellationToken);
}
