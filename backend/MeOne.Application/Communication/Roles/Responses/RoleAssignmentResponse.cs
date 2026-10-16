namespace MeOne.Application.Communication.Roles.Responses;

public sealed record RoleAssignmentResponse(
    ulong Id,
    string Name,
    string Description,
    ulong? ScopeId,
    string? ScopeCode,
    string? ScopeName);
