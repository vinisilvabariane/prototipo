using MeOne.Domain.Entities;
using MeOne.Domain.Enums;

namespace MeOne.Domain.Interfaces;

public interface IWorkflowRepository : IRepository<Workflow, ulong>
{
    Task<int> CountAsync(WorkflowListFilter filter, ulong actorId, string? search, CancellationToken cancellationToken);

    Task<IReadOnlyList<Workflow>> SearchAsync(
        WorkflowListFilter filter,
        ulong actorId,
        string? search,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<WorkflowStatus, int>> CountByStatusAsync(CancellationToken cancellationToken);
}
