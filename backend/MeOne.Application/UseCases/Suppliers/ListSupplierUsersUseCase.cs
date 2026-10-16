using FluentResults;
using MeOne.Application.Communication.Suppliers.Responses;
using MeOne.Application.Interfaces.Suppliers;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Suppliers;

public sealed class ListSupplierUsersUseCase(IUserRepository users) : IListSupplierUsersUseCase
{
    public async Task<Result<IReadOnlyList<SupplierUserResponse>>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var active = await users.ListActiveAsync(cancellationToken);

        return Result.Ok<IReadOnlyList<SupplierUserResponse>>(
            [.. active.Select(user => new SupplierUserResponse(user.Id, user.Name, user.Department?.Name ?? string.Empty))]);
    }
}
