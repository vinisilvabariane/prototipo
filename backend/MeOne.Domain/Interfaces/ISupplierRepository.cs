using MeOne.Domain.Entities;
using MeOne.Domain.Enums;

namespace MeOne.Domain.Interfaces;

public interface ISupplierRepository : IRepository<Supplier, ulong>
{
    Task<bool> SapCodeExistsAsync(string normalizedCode, MultiBranch multiBranch, ulong? exceptId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Supplier>> ListAsync(
        string? search,
        SupplierOrigin? origin,
        MultiBranch? multiBranch,
        SupplierCriticality? criticality,
        CancellationToken cancellationToken);
}
