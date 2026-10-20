using FluentResults;
using MeOne.Application.Communication.Helpdesk.Requests;
using MeOne.Application.Communication.Helpdesk.Responses;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Helpdesk;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Helpdesk;

public sealed class CreateHelpdeskSectorUseCase(
    IHelpdeskSectorRepository sectors,
    IRoleRepository roles,
    IUserRepository users,
    ICurrentActor currentActor) : ICreateHelpdeskSectorUseCase
{
    public async Task<Result<HelpdeskSectorDetailResponse>> ExecuteAsync(SaveHelpdeskSectorRequest request, CancellationToken cancellationToken)
    {
        var actor = await HelpdeskActor.ResolveAsync(currentActor, users, cancellationToken);
        if (actor.IsFailed)
            return actor.ToResult<HelpdeskSectorDetailResponse>();

        var draft = HelpdeskMapping.ToDraft(request);

        if (HelpdeskSector.Check(draft) is { } problem)
            return Result.Fail<HelpdeskSectorDetailResponse>(problem);

        if (await HelpdeskRoleCheck.UnknownAsync(roles, draft.AttendantRoleIds, cancellationToken))
            return Result.Fail<HelpdeskSectorDetailResponse>(HelpdeskErrors.RoleUnknown);

        var name = HelpdeskSector.NormalizeName(draft.Name);

        if (await sectors.NameExistsAsync(name, null, cancellationToken))
            return Result.Fail<HelpdeskSectorDetailResponse>(HelpdeskErrors.NameTaken(name));

        var created = HelpdeskSector.Create(draft, DateTime.UtcNow);
        await sectors.AddAsync(created, cancellationToken);

        return await sectors.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok(HelpdeskMapping.ToDetail(created, actor.Value))
            : Result.Fail<HelpdeskSectorDetailResponse>(HelpdeskErrors.NothingPersisted);
    }
}
