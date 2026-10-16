using FluentResults;
using MeOne.Application.Communication.Suppliers.Requests;
using MeOne.Application.Communication.Suppliers.Responses;

namespace MeOne.Application.Interfaces.Suppliers;

public interface IAddSupplierMaterialUseCase
{
    Task<Result<SupplierMaterialResponse>> ExecuteAsync(ulong supplierId, SaveSupplierMaterialRequest request, CancellationToken cancellationToken);
}
