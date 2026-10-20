using FluentResults;
using MeOne.Application.Communication.Helpdesk.Responses;
using MeOne.Application.Interfaces.Helpdesk;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Helpdesk;

public sealed class ListHelpdeskRolesUseCase(IRoleRepository roles) : IListHelpdeskRolesUseCase
{
    public async Task<Result<IReadOnlyList<HelpdeskRoleResponse>>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var list = await roles.ListOrderedAsync(cancellationToken);

        return Result.Ok<IReadOnlyList<HelpdeskRoleResponse>>([.. list.Select(role => new HelpdeskRoleResponse(role.Id, role.Name, role.Description))]);
    }
}
