using FluentResults;


namespace MeOne.Application.Interfaces.Suppliers;

public interface IRemoveSupplierMaterialUseCase
{
    Task<Result> ExecuteAsync(ulong supplierId, ulong itemId, CancellationToken cancellationToken);
}
