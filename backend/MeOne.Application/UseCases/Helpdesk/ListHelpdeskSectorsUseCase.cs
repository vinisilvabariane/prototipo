using FluentResults;
using MeOne.Application.Communication.Helpdesk.Requests;
using MeOne.Application.Communication.Helpdesk.Responses;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Helpdesk;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Helpdesk;

public sealed class ListHelpdeskSectorsUseCase(
    IHelpdeskSectorRepository sectors,
    IUserRepository users,
    ICurrentActor currentActor) : IListHelpdeskSectorsUseCase
{
    public async Task<Result<HelpdeskSectorListResponse>> ExecuteAsync(HelpdeskSectorsQuery query, CancellationToken cancellationToken)
    {
        var actor = await HelpdeskActor.ResolveAsync(currentActor, users, cancellationToken);
        if (actor.IsFailed)
            return actor.ToResult<HelpdeskSectorListResponse>();

        var list = await sectors.ListAsync(query.IncludeInactive && actor.Value.CanManage, cancellationToken);

        return Result.Ok(new HelpdeskSectorListResponse([.. list.Select(sector => HelpdeskMapping.ToSummary(sector, actor.Value))], actor.Value.CanManage));
    }
}
