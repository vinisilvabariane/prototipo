using MeOne.Application.Communication.Roles.Responses;

namespace MeOne.Application.Communication.Auth.Responses;

public sealed record MyAccessResponse(
    ulong UserId,
    uint Registration,
    string Name,
    string? Email,
    string Department,
    bool IsAdministrator,
    IReadOnlyList<RoleAssignmentResponse> Roles,
    IReadOnlyList<PermissionGrantResponse> Permissions);
