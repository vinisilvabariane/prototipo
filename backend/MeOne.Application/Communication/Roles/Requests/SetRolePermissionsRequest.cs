namespace MeOne.Application.Communication.Roles.Requests;

public sealed record SetRolePermissionsRequest(IReadOnlyList<string> Permissions);
