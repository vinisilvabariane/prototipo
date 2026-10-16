namespace MeOne.Application.Communication.Roles.Responses;

public sealed record PermissionResponse(string Code, string Description, bool GlobalOnly);
