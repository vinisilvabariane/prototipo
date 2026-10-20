using FluentResults;
using MeOne.Application.Communication.Helpdesk.Requests;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces.Helpdesk;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Helpdesk;

public sealed class UpdateHelpdeskFieldTemplateUseCase(IHelpdeskFieldTemplateRepository templates) : IUpdateHelpdeskFieldTemplateUseCase
{
    public async Task<Result> ExecuteAsync(ulong id, SaveHelpdeskFieldTemplateRequest request, CancellationToken cancellationToken)
    {
        var template = await templates.GetByIdAsync(id, cancellationToken);
        if (template is null)
            return Result.Fail(new NotFoundError(HelpdeskErrors.TemplateNotFound));

        var draft = HelpdeskMapping.ToDraft(request);

        if (HelpdeskFieldTemplate.Check(draft) is { } problem)
            return Result.Fail(problem);

        var name = HelpdeskFieldTemplate.NormalizeName(draft.Name);

        if (await templates.NameExistsAsync(name, id, cancellationToken))
            return Result.Fail(HelpdeskErrors.NameTaken(name));

        template.Update(draft, DateTime.UtcNow);
        templates.Update(template);

        return await templates.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok()
            : Result.Fail(HelpdeskErrors.NothingPersisted);
    }
}
