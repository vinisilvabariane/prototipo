using IrisWeb.Domain.Entities;

namespace IrisWeb.Domain.Interfaces;

public interface IDepartmentRepository : IRepository<Department, int>
{
    /// <summary>Recebe o nome já normalizado (<see cref="Department.Normalize"/>).</summary>
    Task<Department?> GetByNameAsync(string normalizedName, CancellationToken cancellationToken);

    Task<bool> NameExistsAsync(string normalizedName, int? exceptDepartmentId, CancellationToken cancellationToken);

    /// <summary>Conta também usuários desativados: a FK no banco não some com o soft delete.</summary>
    Task<bool> InUseAsync(int departmentId, CancellationToken cancellationToken);

    Task<IReadOnlyList<Department>> ListOrderedAsync(CancellationToken cancellationToken);
}
