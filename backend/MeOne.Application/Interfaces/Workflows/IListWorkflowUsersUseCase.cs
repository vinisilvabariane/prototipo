using FluentResults;
using MeOne.Application.Communication.Workflows.Responses;

namespace MeOne.Application.Interfaces.Workflows;

public interface IListWorkflowUsersUseCase
{
    Task<Result<IReadOnlyList<WorkflowUserResponse>>> ExecuteAsync(CancellationToken cancellationToken);
}
