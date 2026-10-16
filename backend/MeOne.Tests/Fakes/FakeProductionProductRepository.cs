using MeOne.Domain.Entities;
using MeOne.Domain.Interfaces;

namespace MeOne.Tests.Fakes;

public sealed class FakeProductionProductRepository : InMemoryRepository<ProductionProduct, ulong>, IProductionProductRepository
{
    public Task<bool> CodeExistsAsync(string value, ulong? exceptId, CancellationToken cancellationToken) =>
        Task.FromResult(Items.Any(item => item.Code == value && item.Id != exceptId));

    public Task<IReadOnlyList<ProductionProduct>> ListAsync(bool includeInactive, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ProductionProduct>>([.. Items.Where(item => includeInactive || item.Active).OrderBy(item => item.Code)]);

    public Task<IReadOnlyList<ProductionProduct>> GetByIdsAsync(IReadOnlyCollection<ulong> ids, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<ProductionProduct>>([.. Items.Where(item => ids.Contains(item.Id))]);
}
