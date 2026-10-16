namespace MeOne.Application.Communication.Roles.Requests;

public sealed record AssignRoleRequest(ulong UserId, ulong RoleId, ulong? ScopeId);
