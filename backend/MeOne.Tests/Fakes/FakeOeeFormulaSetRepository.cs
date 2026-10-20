using MeOne.Domain.Entities;
using MeOne.Domain.Interfaces;

namespace MeOne.Tests.Fakes;

public sealed class FakeOeeFormulaSetRepository : InMemoryRepository<OeeFormulaSet, ulong>, IOeeFormulaSetRepository
{
    public Task<bool> NameExistsAsync(string name, ulong? exceptId, CancellationToken cancellationToken) =>
        Task.FromResult(Items.Any(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase) && item.Id != exceptId));

    public Task<OeeFormulaSet?> GetActiveAsync(CancellationToken cancellationToken) =>
        Task.FromResult(Items.FirstOrDefault(item => item.IsActive));

    public Task<IReadOnlyList<OeeFormulaSet>> ListAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<OeeFormulaSet>>([.. Items.OrderBy(item => item.Name)]);
}
