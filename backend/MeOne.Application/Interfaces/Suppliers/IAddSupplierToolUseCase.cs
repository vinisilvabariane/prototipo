using FluentResults;
using MeOne.Application.Communication.Suppliers.Requests;
using MeOne.Application.Communication.Suppliers.Responses;

namespace MeOne.Application.Interfaces.Suppliers;

public interface IAddSupplierToolUseCase
{
    Task<Result<SupplierToolResponse>> ExecuteAsync(ulong supplierId, SaveSupplierToolRequest request, CancellationToken cancellationToken);
}
