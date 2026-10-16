using MeOne.Domain.Entities;
using MeOne.Domain.Enums;
using MeOne.Domain.Interfaces;

namespace MeOne.Tests.Fakes;

public class FakeWorkflowRepository : InMemoryRepository<Workflow, ulong>, IWorkflowRepository
{
    private ulong _lastChildIdentity;

    public Task<int> CountAsync(
        WorkflowListFilter filter,
        ulong actorId,
        string? search,
        CancellationToken cancellationToken) =>
        Task.FromResult(Matching(filter, actorId, search).Count());

    public Task<IReadOnlyList<Workflow>> SearchAsync(
        WorkflowListFilter filter,
        ulong actorId,
        string? search,
        int skip,
        int take,
        CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Workflow>>(
        [
            .. Matching(filter, actorId, search)
                .OrderByDescending(workflow => workflow.CreatedAt)
                .ThenByDescending(workflow => workflow.Id)
                .Skip(skip)
                .Take(take),
        ]);

    public Task<IReadOnlyDictionary<WorkflowStatus, int>> CountByStatusAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyDictionary<WorkflowStatus, int>>(
            Items.GroupBy(workflow => workflow.Status).ToDictionary(group => group.Key, group => group.Count()));

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        foreach (var workflow in Items)
        {
            foreach (var assignee in workflow.Assignees.Where(item => item.Id == 0))
                assignee.Id = ++_lastChildIdentity;

            foreach (var step in workflow.Steps.Where(item => item.Id == 0))
                step.Id = ++_lastChildIdentity;

            foreach (var attachment in workflow.Attachments.Where(item => item.Id == 0))
                attachment.Id = ++_lastChildIdentity;

            foreach (var message in workflow.Messages.Where(item => item.Id == 0))
                message.Id = ++_lastChildIdentity;
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    private IEnumerable<Workflow> Matching(WorkflowListFilter filter, ulong actorId, string? search)
    {
        var query = filter switch
        {
            WorkflowListFilter.Mine => Items.Where(workflow => workflow.CreatorId == actorId),
            WorkflowListFilter.Participating => Items.Where(workflow => workflow.IsAssignee(actorId)),
            _ => Items.AsEnumerable(),
        };

        if (string.IsNullOrWhiteSpace(search))
            return query;

        var term = search.Trim();

        return query.Where(workflow =>
            workflow.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            workflow.Description.Contains(term, StringComparison.OrdinalIgnoreCase));
    }
}
