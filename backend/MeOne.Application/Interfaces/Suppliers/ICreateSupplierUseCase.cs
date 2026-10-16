using FluentResults;
using MeOne.Application.Communication.Suppliers.Requests;
using MeOne.Application.Communication.Suppliers.Responses;

namespace MeOne.Application.Interfaces.Suppliers;

public interface ICreateSupplierUseCase
{
    Task<Result<SupplierSummaryResponse>> ExecuteAsync(SaveSupplierRequest request, CancellationToken cancellationToken);
}
