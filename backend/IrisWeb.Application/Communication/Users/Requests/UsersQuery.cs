namespace IrisWeb.Application.Communication.Users.Requests;

public sealed record UsersQuery
{
    /// <summary>Começa em 0.</summary>
    public int Page { get; init; }

    public int PageSize { get; init; } = 10;

    public string? Search { get; init; }
}
