using MeOne.Domain.Entities;

namespace MeOne.Domain.Interfaces;

public interface IProductionLineRepository : IRepository<ProductionLine, ulong>
{
    Task<bool> NameExistsAsync(string name, ulong? exceptId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ProductionLine>> ListAsync(bool includeInactive, CancellationToken cancellationToken);

    Task<IReadOnlyList<ProductionLine>> GetByIdsAsync(IReadOnlyCollection<ulong> ids, CancellationToken cancellationToken);
}
