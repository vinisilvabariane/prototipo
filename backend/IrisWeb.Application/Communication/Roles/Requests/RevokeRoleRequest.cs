namespace IrisWeb.Application.Communication.Roles.Requests;

public sealed record RevokeRoleRequest(ulong UserId, ulong RoleId);
