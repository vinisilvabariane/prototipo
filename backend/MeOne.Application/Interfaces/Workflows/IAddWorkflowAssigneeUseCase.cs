using FluentResults;
using MeOne.Application.Communication.Workflows.Requests;
using MeOne.Application.Communication.Workflows.Responses;

namespace MeOne.Application.Interfaces.Workflows;

public interface IAddWorkflowAssigneeUseCase
{
    Task<Result<WorkflowAssigneeResponse>> ExecuteAsync(
        ulong id,
        AddWorkflowAssigneeRequest request,
        CancellationToken cancellationToken);
}
