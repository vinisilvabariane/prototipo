using FluentResults;
using MeOne.Application.Communication.Workflows.Requests;

namespace MeOne.Application.Interfaces.Workflows;

public interface IUpdateWorkflowUseCase
{
    Task<Result> ExecuteAsync(ulong id, UpdateWorkflowRequest request, CancellationToken cancellationToken);
}
