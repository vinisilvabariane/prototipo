using MeOne.Domain.Entities;
using MeOne.Domain.Enums;

namespace MeOne.Domain.Interfaces;

public interface IProductionRunRepository : IRepository<ProductionRun, ulong>
{
    Task<bool> LineHasActiveRunAsync(ulong lineId, CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<ulong, ulong>> FindActiveRunsOfUsersAsync(
        IReadOnlyCollection<ulong> userIds,
        ulong exceptRunId,
        CancellationToken cancellationToken);

    Task<ProductionRun?> GetLastClosedOfLayoutAsync(ulong layoutId, CancellationToken cancellationToken);

    Task<int> CountAsync(RunStatus? status, ulong? lineId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ProductionRun>> SearchAsync(RunStatus? status, ulong? lineId, int skip, int take, CancellationToken cancellationToken);
}
