using FluentResults;


namespace MeOne.Application.Interfaces.Suppliers;

public interface IRemoveSupplierDocumentUseCase
{
    Task<Result> ExecuteAsync(ulong supplierId, ulong documentId, CancellationToken cancellationToken);
}
