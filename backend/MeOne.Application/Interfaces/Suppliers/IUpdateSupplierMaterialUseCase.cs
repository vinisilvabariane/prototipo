using FluentResults;
using MeOne.Application.Communication.Suppliers.Requests;

namespace MeOne.Application.Interfaces.Suppliers;

public interface IUpdateSupplierMaterialUseCase
{
    Task<Result> ExecuteAsync(ulong supplierId, ulong itemId, SaveSupplierMaterialRequest request, CancellationToken cancellationToken);
}
