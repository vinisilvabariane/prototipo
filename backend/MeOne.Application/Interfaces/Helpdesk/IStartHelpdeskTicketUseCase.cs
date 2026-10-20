using FluentResults;
using MeOne.Application.Communication.Helpdesk;
using MeOne.Application.Communication.Helpdesk.Requests;
using MeOne.Application.Communication.Helpdesk.Responses;

namespace MeOne.Application.Interfaces.Helpdesk;

public interface IStartHelpdeskTicketUseCase
{
    Task<Result> ExecuteAsync(ulong id, CancellationToken cancellationToken);
}
