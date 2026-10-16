using MeOne.Domain.Entities;
using MeOne.Domain.Interfaces;

namespace MeOne.Tests.Fakes;

public sealed class FakeProductionLineRepository : InMemoryRepository<ProductionLine, ulong>, IProductionLineRepository
{
    public Task<bool> NameExistsAsync(string value, ulong? exceptId, CancellationToken cancellationToken) =>
        Task.FromResult(Items.Any(item => string.Equals(item.Name, value, StringComparison.OrdinalIgnoreCase) && item.Id != exceptId));

    public Task<IReadOnlyList<ProductionLine>> ListAsync(bool includeInactive, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ProductionLine>>([.. Items.Where(item => includeInactive || item.Active).OrderBy(item => item.Name)]);

    public Task<IReadOnlyList<ProductionLine>> GetByIdsAsync(IReadOnlyCollection<ulong> ids, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ProductionLine>>([.. Items.Where(item => ids.Contains(item.Id))]);
}
