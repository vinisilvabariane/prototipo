using FluentResults;
using MeOne.Application.Communication.Common.Responses;
using MeOne.Application.Communication.Workflows.Requests;
using MeOne.Application.Communication.Workflows.Responses;

namespace MeOne.Application.Interfaces.Workflows;

public interface IListWorkflowsUseCase
{
    Task<Result<PagedResponse<WorkflowSummaryResponse>>> ExecuteAsync(WorkflowsQuery query, CancellationToken cancellationToken);
}
