using FluentResults;
using MeOne.Application.Communication.Suppliers.Requests;
using MeOne.Application.Communication.Suppliers.Responses;

namespace MeOne.Application.Interfaces.Suppliers;

public interface IListSuppliersUseCase
{
    Task<Result<SupplierPageResponse>> ExecuteAsync(SuppliersQuery query, CancellationToken cancellationToken);
}
