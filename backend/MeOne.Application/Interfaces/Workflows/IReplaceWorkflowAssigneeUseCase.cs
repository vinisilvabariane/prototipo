using FluentResults;
using MeOne.Application.Communication.Workflows.Requests;
using MeOne.Application.Communication.Workflows.Responses;

namespace MeOne.Application.Interfaces.Workflows;

public interface IReplaceWorkflowAssigneeUseCase
{
    Task<Result<WorkflowActionResponse>> ExecuteAsync(
        ulong id,
        ulong userId,
        ReplaceWorkflowAssigneeRequest request,
        CancellationToken cancellationToken);
}
