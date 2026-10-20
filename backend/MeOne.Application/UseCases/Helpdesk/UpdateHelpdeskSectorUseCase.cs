using FluentResults;
using MeOne.Application.Communication.Helpdesk.Requests;
using MeOne.Application.Interfaces.Helpdesk;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Helpdesk;

public sealed class UpdateHelpdeskSectorUseCase(IHelpdeskSectorRepository sectors, IRoleRepository roles) : IUpdateHelpdeskSectorUseCase
{
    public async Task<Result> ExecuteAsync(ulong id, SaveHelpdeskSectorRequest request, CancellationToken cancellationToken)
    {
        var sector = await HelpdeskLookups.SectorAsync(sectors, id, cancellationToken);
        if (sector.IsFailed)
            return sector.ToResult();

        var draft = HelpdeskMapping.ToDraft(request);

        if (HelpdeskSector.Check(draft) is { } problem)
            return Result.Fail(problem);

        if (await HelpdeskRoleCheck.UnknownAsync(roles, draft.AttendantRoleIds, cancellationToken))
            return Result.Fail(HelpdeskErrors.RoleUnknown);

        var name = HelpdeskSector.NormalizeName(draft.Name);

        if (await sectors.NameExistsAsync(name, id, cancellationToken))
            return Result.Fail(HelpdeskErrors.NameTaken(name));

        sector.Value.Update(draft, DateTime.UtcNow);
        sectors.Update(sector.Value);

        return await sectors.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok()
            : Result.Fail(HelpdeskErrors.NothingPersisted);
    }
}
