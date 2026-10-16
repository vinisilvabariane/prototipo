using FluentResults;


namespace MeOne.Application.Interfaces.Suppliers;

public interface IRemoveSupplierToolUseCase
{
    Task<Result> ExecuteAsync(ulong supplierId, ulong itemId, CancellationToken cancellationToken);
}
