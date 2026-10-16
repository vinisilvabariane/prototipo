using FluentResults;
using MeOne.Application.Communication.Suppliers.Requests;

namespace MeOne.Application.Interfaces.Suppliers;

public interface IUpdateSupplierToolUseCase
{
    Task<Result> ExecuteAsync(ulong supplierId, ulong itemId, SaveSupplierToolRequest request, CancellationToken cancellationToken);
}
