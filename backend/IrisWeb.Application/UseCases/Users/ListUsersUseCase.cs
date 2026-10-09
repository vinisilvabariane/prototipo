using FluentResults;
using IrisWeb.Application.Communication.Common.Responses;
using IrisWeb.Application.Communication.Users.Requests;
using IrisWeb.Application.Communication.Users.Responses;
using IrisWeb.Application.Interfaces.Users;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.Users;

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
