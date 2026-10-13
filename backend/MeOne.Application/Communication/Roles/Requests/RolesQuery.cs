namespace MeOne.Application.Communication.Roles.Requests;

public sealed record RolesQuery
{
    public int Page { get; init; }

    public int PageSize { get; init; } = 10;
}
