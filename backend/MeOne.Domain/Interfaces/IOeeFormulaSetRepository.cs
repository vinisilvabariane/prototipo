using MeOne.Domain.Entities;

namespace MeOne.Domain.Interfaces;

public interface IOeeFormulaSetRepository : IRepository<OeeFormulaSet, ulong>
{
    Task<bool> NameExistsAsync(string name, ulong? exceptId, CancellationToken cancellationToken);

    Task<OeeFormulaSet?> GetActiveAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<OeeFormulaSet>> ListAsync(CancellationToken cancellationToken);
}
