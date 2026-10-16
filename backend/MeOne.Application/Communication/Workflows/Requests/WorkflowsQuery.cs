using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Workflows.Requests;

public sealed record WorkflowsQuery
{
    public int Page { get; init; }

    public int PageSize { get; init; } = 10;

    public string? Search { get; init; }

    public WorkflowListFilter Filter { get; init; } = WorkflowListFilter.All;
}
