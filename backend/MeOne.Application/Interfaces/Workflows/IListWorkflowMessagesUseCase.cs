using FluentResults;
using MeOne.Application.Communication.Workflows.Responses;

namespace MeOne.Application.Interfaces.Workflows;

public interface IListWorkflowMessagesUseCase
{
    Task<Result<IReadOnlyList<WorkflowMessageResponse>>> ExecuteAsync(
        ulong id,
        ulong stepId,
        CancellationToken cancellationToken);
}
