using FluentResults;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces.Helpdesk;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Helpdesk;

public sealed class DeleteHelpdeskFieldTemplateUseCase(IHelpdeskFieldTemplateRepository templates) : IDeleteHelpdeskFieldTemplateUseCase
{
    public async Task<Result> ExecuteAsync(ulong id, CancellationToken cancellationToken)
    {
        var template = await templates.GetByIdAsync(id, cancellationToken);
        if (template is null)
            return Result.Fail(new NotFoundError(HelpdeskErrors.TemplateNotFound));

        templates.Delete(template);

        return await templates.SaveChangesAsync(cancellationToken) > 0
            ? Result.Ok()
            : Result.Fail(HelpdeskErrors.NothingPersisted);
    }
}
