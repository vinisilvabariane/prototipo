using FluentResults;
using MeOne.Application.Communication.Suppliers.Requests;

namespace MeOne.Application.Interfaces.Suppliers;

public interface IUpdateSupplierAuditUseCase
{
    Task<Result> ExecuteAsync(ulong supplierId, ulong itemId, SaveAuditRequest request, CancellationToken cancellationToken);
}
