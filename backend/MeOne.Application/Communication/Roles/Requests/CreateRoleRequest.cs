namespace MeOne.Application.Communication.Roles.Requests;

public sealed record CreateRoleRequest(string Name, string Description, IReadOnlyList<string>? Permissions);
