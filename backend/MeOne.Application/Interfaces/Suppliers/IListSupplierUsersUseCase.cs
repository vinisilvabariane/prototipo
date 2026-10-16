using FluentResults;
using MeOne.Application.Communication.Suppliers.Responses;

namespace MeOne.Application.Interfaces.Suppliers;

public interface IListSupplierUsersUseCase
{
    Task<Result<IReadOnlyList<SupplierUserResponse>>> ExecuteAsync(CancellationToken cancellationToken);
}
