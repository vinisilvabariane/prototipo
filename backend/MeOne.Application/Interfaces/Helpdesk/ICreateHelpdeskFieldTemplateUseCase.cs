using FluentResults;
using MeOne.Application.Communication.Helpdesk;
using MeOne.Application.Communication.Helpdesk.Requests;
using MeOne.Application.Communication.Helpdesk.Responses;

namespace MeOne.Application.Interfaces.Helpdesk;

public interface ICreateHelpdeskFieldTemplateUseCase
{
    Task<Result<HelpdeskFieldTemplateResponse>> ExecuteAsync(SaveHelpdeskFieldTemplateRequest request, CancellationToken cancellationToken);
}
