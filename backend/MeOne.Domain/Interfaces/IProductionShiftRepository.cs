using MeOne.Domain.Entities;

namespace MeOne.Domain.Interfaces;

public interface IProductionShiftRepository : IRepository<ProductionShift, ulong>
{
    Task<bool> NameExistsAsync(string name, ulong? exceptId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ProductionShift>> ListAsync(bool includeInactive, CancellationToken cancellationToken);

    Task<IReadOnlyList<ProductionShift>> GetByIdsAsync(IReadOnlyCollection<ulong> ids, CancellationToken cancellationToken);
}
