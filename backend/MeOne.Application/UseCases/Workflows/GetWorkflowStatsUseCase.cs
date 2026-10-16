using FluentResults;
using MeOne.Application.Communication.Workflows.Responses;
using MeOne.Application.Interfaces.Workflows;
using MeOne.Domain.Enums;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Workflows;

public sealed class GetWorkflowStatsUseCase(IWorkflowRepository workflows) : IGetWorkflowStatsUseCase
{
    public async Task<Result<WorkflowStatsResponse>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var counts = await workflows.CountByStatusAsync(cancellationToken);

        return Result.Ok(new WorkflowStatsResponse(
            counts.Values.Sum(),
            counts.GetValueOrDefault(WorkflowStatus.InProgress),
            counts.GetValueOrDefault(WorkflowStatus.Approved),
            counts.GetValueOrDefault(WorkflowStatus.Rejected)));
    }
}
