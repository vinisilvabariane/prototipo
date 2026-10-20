using FluentResults;
using MeOne.Application.Communication.Helpdesk;
using MeOne.Application.Communication.Helpdesk.Requests;
using MeOne.Application.Communication.Helpdesk.Responses;

namespace MeOne.Application.Interfaces.Helpdesk;

public interface IListHelpdeskFieldTemplatesUseCase
{
    Task<Result<IReadOnlyList<HelpdeskFieldTemplateResponse>>> ExecuteAsync(CancellationToken cancellationToken);
}
