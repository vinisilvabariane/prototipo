namespace IrisWeb.Application.Communication.Roles.Requests;

public sealed record RolesQuery
{
    /// <summary>Começa em 0, igual à listagem de usuários.</summary>
    public int Page { get; init; }

    public int PageSize { get; init; } = 10;
}
