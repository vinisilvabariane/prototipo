using FluentResults;
using MeOne.Application.Communication.Helpdesk;
using MeOne.Application.Communication.Helpdesk.Requests;
using MeOne.Application.Communication.Helpdesk.Responses;

namespace MeOne.Application.Interfaces.Helpdesk;

public interface ICreateHelpdeskSectorUseCase
{
    Task<Result<HelpdeskSectorDetailResponse>> ExecuteAsync(SaveHelpdeskSectorRequest request, CancellationToken cancellationToken);
}
