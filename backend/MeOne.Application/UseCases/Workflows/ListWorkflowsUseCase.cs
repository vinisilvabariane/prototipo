using FluentResults;
using MeOne.Application.Communication.Common.Responses;
using MeOne.Application.Communication.Workflows.Requests;
using MeOne.Application.Communication.Workflows.Responses;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.Workflows;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.Workflows;

public sealed class ListWorkflowsUseCase(IWorkflowRepository workflows, ICurrentActor currentActor) : IListWorkflowsUseCase
{
    public async Task<Result<PagedResponse<WorkflowSummaryResponse>>> ExecuteAsync(
        WorkflowsQuery query,
        CancellationToken cancellationToken)
    {
        var actorId = await WorkflowsMapping.ActorIdAsync(currentActor, cancellationToken);
        if (actorId.IsFailed)
            return actorId.ToResult<PagedResponse<WorkflowSummaryResponse>>();

        var search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim();

        var total = await workflows.CountAsync(query.Filter, actorId.Value, search, cancellationToken);
        var page = await workflows.SearchAsync(
            query.Filter,
            actorId.Value,
            search,
            query.Page * query.PageSize,
            query.PageSize,
            cancellationToken);

        return Result.Ok(new PagedResponse<WorkflowSummaryResponse>(
            [.. page.Select(WorkflowsMapping.ToSummary)],
            total,
            query.Page,
            query.PageSize));
    }
}
