using FluentResults;
using MeOne.Application.Communication.Suppliers.Responses;

namespace MeOne.Application.Interfaces.Suppliers;

public interface IGetSupplierUseCase
{
    Task<Result<SupplierDetailResponse>> ExecuteAsync(ulong id, CancellationToken cancellationToken);
}
