namespace MeOne.Application.Communication.Roles.Responses;

public sealed record RoleResponse(
    ulong Id,
    string Name,
    string Description,
    bool IsAdministrator,
    IReadOnlyList<string> Permissions);
