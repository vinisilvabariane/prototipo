using IrisWeb.Application.Communication.Roles.Responses;
using IrisWeb.Domain.Entities;

namespace IrisWeb.Application.UseCases.Roles;

internal static class RolesMapping
{
    public static RoleResponse ToResponse(Role role) => new(role.Id, role.Name, role.Description);
}
