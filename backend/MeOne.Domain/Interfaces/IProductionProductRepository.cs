using MeOne.Domain.Entities;

namespace MeOne.Domain.Interfaces;

public interface IProductionProductRepository : IRepository<ProductionProduct, ulong>
{
    Task<bool> CodeExistsAsync(string normalizedCode, ulong? exceptId, CancellationToken cancellationToken);

    Task<IReadOnlyList<ProductionProduct>> ListAsync(bool includeInactive, CancellationToken cancellationToken);

    Task<IReadOnlyList<ProductionProduct>> GetByIdsAsync(IReadOnlyCollection<ulong> ids, CancellationToken cancellationToken);
}
