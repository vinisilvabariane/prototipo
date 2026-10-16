using FluentResults;
using MeOne.Application.Communication.Workflows.Responses;

namespace MeOne.Application.Interfaces.Workflows;

public interface IGetWorkflowStatsUseCase
{
    Task<Result<WorkflowStatsResponse>> ExecuteAsync(CancellationToken cancellationToken);
}
