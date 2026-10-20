using FluentResults;
using MeOne.Application.Communication.Helpdesk.Requests;
using MeOne.Application.Communication.Helpdesk.Responses;
using MeOne.Application.Interfaces.Helpdesk;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Helpdesk;

public sealed class CreateHelpdeskFieldTemplateUseCase(IHelpdeskFieldTemplateRepository templates) : ICreateHelpdeskFieldTemplateUseCase
{
    public async Task<Result<HelpdeskFieldTemplateResponse>> ExecuteAsync(SaveHelpdeskFieldTemplateRequest request, CancellationToken cancellationToken)
    {
        var draft = HelpdeskMapping.ToDraft(request);

        if (HelpdeskFieldTemplate.Check(draft) is { } problem)
            return Result.Fail<HelpdeskFieldTemplateResponse>(problem);

        var name = HelpdeskFieldTemplate.NormalizeName(draft.Name);

        if (await templates.NameExistsAsync(name, null, cancellationToken))
            return Result.Fail<HelpdeskFieldTemplateResponse>(HelpdeskErrors.NameTaken(name));

        var created = HelpdeskFieldTemplate.Create(draft, DateTime.UtcNow);
        await templates.AddAsync(created, cancellationToken);

        return await templates.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok(HelpdeskMapping.ToResponse(created))
            : Result.Fail<HelpdeskFieldTemplateResponse>(HelpdeskErrors.NothingPersisted);
    }
}
