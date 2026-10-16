using FluentResults;
using MeOne.Application.Communication.Suppliers.Requests;
using MeOne.Application.Communication.Suppliers.Responses;

namespace MeOne.Application.Interfaces.Suppliers;

public interface IAddSupplierAuditUseCase
{
    Task<Result<SupplierAuditResponse>> ExecuteAsync(ulong supplierId, SaveAuditRequest request, CancellationToken cancellationToken);
}
