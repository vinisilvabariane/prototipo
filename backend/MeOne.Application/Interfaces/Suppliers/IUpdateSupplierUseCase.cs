using FluentResults;
using MeOne.Application.Communication.Suppliers.Requests;

namespace MeOne.Application.Interfaces.Suppliers;

public interface IUpdateSupplierUseCase
{
    Task<Result> ExecuteAsync(ulong id, SaveSupplierRequest request, CancellationToken cancellationToken);
}
