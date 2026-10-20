using FluentResults;
using MeOne.Application.Communication.Helpdesk.Responses;
using MeOne.Application.Interfaces.Helpdesk;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Helpdesk;

public sealed class ListHelpdeskFieldTemplatesUseCase(IHelpdeskFieldTemplateRepository templates) : IListHelpdeskFieldTemplatesUseCase
{
    public async Task<Result<IReadOnlyList<HelpdeskFieldTemplateResponse>>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var list = await templates.ListAsync(cancellationToken);

        return Result.Ok<IReadOnlyList<HelpdeskFieldTemplateResponse>>([.. list.Select(HelpdeskMapping.ToResponse)]);
    }
}
