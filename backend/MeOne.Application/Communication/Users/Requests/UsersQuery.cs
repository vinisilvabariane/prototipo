namespace MeOne.Application.Communication.Users.Requests;

public sealed record UsersQuery
{
    public int Page { get; init; }

    public int PageSize { get; init; } = 10;

    public string? Search { get; init; }
}
