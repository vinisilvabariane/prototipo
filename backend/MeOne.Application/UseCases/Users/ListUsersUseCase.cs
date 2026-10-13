using FluentResults;
using MeOne.Application.Communication.Common.Responses;
using MeOne.Application.Communication.Users.Requests;
using MeOne.Application.Communication.Users.Responses;
using MeOne.Application.Interfaces.Users;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Users;

public sealed class ListUsersUseCase(IUserRepository users) : IListUsersUseCase
{
    public async Task<Result<PagedResponse<UserResponse>>> ExecuteAsync(
        UsersQuery query,
        CancellationToken cancellationToken)
    {
        var search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim();

        var total = await users.CountAsync(search, cancellationToken);
        var page = await users.SearchAsync(search, query.Page * query.PageSize, query.PageSize, cancellationToken);

        return Result.Ok(new PagedResponse<UserResponse>(
            [.. page.Select(UsersMapping.ToResponse)],
            total,
            query.Page,
            query.PageSize));
    }
}
