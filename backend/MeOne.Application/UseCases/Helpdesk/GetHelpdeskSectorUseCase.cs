using FluentResults;
using MeOne.Application.Communication.Helpdesk.Responses;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Helpdesk;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Helpdesk;

public sealed class GetHelpdeskSectorUseCase(
    IHelpdeskSectorRepository sectors,
    IUserRepository users,
    ICurrentActor currentActor) : IGetHelpdeskSectorUseCase
{
    public async Task<Result<HelpdeskSectorDetailResponse>> ExecuteAsync(ulong id, CancellationToken cancellationToken)
    {
        var actor = await HelpdeskActor.ResolveAsync(currentActor, users, cancellationToken);
        if (actor.IsFailed)
            return actor.ToResult<HelpdeskSectorDetailResponse>();

        var sector = await HelpdeskLookups.SectorAsync(sectors, id, cancellationToken);
        if (sector.IsFailed)
            return sector.ToResult<HelpdeskSectorDetailResponse>();

        return Result.Ok(HelpdeskMapping.ToDetail(sector.Value, actor.Value));
    }
}
