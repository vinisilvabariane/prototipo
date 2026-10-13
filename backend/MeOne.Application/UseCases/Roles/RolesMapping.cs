using MeOne.Application.Communication.Roles.Responses;
using MeOne.Domain.Entities;

namespace MeOne.Application.UseCases.Roles;

internal static class RolesMapping
{
    public static RoleResponse ToResponse(Role role) => new(role.Id, role.Name, role.Description);
}
