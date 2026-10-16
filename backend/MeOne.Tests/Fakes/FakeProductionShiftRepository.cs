using MeOne.Domain.Entities;
using MeOne.Domain.Interfaces;

namespace MeOne.Tests.Fakes;

public sealed class FakeProductionShiftRepository : InMemoryRepository<ProductionShift, ulong>, IProductionShiftRepository
{
    public Task<bool> NameExistsAsync(string value, ulong? exceptId, CancellationToken cancellationToken) =>
        Task.FromResult(Items.Any(item => string.Equals(item.Name, value, StringComparison.OrdinalIgnoreCase) && item.Id != exceptId));

    public Task<IReadOnlyList<ProductionShift>> ListAsync(bool includeInactive, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ProductionShift>>([.. Items.Where(item => includeInactive || item.Active).OrderBy(item => item.Name)]);

    public Task<IReadOnlyList<ProductionShift>> GetByIdsAsync(IReadOnlyCollection<ulong> ids, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ProductionShift>>([.. Items.Where(item => ids.Contains(item.Id))]);
}
