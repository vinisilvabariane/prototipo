using MeOne.Domain.Entities;

namespace MeOne.Domain.Interfaces;

public interface IDepartmentRepository : IRepository<Department, int>
{
    Task<Department?> GetByNameAsync(string normalizedName, CancellationToken cancellationToken);

    Task<bool> NameExistsAsync(string normalizedName, int? exceptDepartmentId, CancellationToken cancellationToken);

    Task<bool> InUseAsync(int departmentId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Department>> ListOrderedAsync(CancellationToken cancellationToken);
}
