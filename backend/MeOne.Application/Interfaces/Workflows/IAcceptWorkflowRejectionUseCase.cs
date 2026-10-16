using FluentResults;
using MeOne.Application.Communication.Workflows.Requests;
using MeOne.Application.Communication.Workflows.Responses;

namespace MeOne.Application.Interfaces.Workflows;

public interface IAcceptWorkflowRejectionUseCase
{
    Task<Result<WorkflowActionResponse>> ExecuteAsync(
        ulong id,
        WorkflowJustificationRequest request,
        CancellationToken cancellationToken);
}
