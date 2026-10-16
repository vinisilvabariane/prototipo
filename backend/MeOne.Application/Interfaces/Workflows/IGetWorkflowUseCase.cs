using FluentResults;
using MeOne.Application.Communication.Workflows.Responses;

namespace MeOne.Application.Interfaces.Workflows;

public interface IGetWorkflowUseCase
{
    Task<Result<WorkflowDetailResponse>> ExecuteAsync(ulong id, CancellationToken cancellationToken);
}
