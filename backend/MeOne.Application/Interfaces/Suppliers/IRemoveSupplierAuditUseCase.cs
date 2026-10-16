using FluentResults;


namespace MeOne.Application.Interfaces.Suppliers;

public interface IRemoveSupplierAuditUseCase
{
    Task<Result> ExecuteAsync(ulong supplierId, ulong itemId, CancellationToken cancellationToken);
}
