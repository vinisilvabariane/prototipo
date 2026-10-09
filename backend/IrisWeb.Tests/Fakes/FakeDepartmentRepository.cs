using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Tests.Fakes;

public sealed class FakeDepartmentRepository(FakeUserRepository? users = null)
    : InMemoryRepository<Department, int>, IDepartmentRepository
{
    public Task<Department?> GetByNameAsync(string normalizedName, CancellationToken cancellationToken) =>
        Task.FromResult(Items.FirstOrDefault(department => department.Name == normalizedName));

    public Task<bool> NameExistsAsync(string normalizedName, int? exceptDepartmentId, CancellationToken cancellationToken) =>
        Task.FromResult(Items.Any(department => department.Name == normalizedName && department.Id != exceptDepartmentId));

    public Task<bool> InUseAsync(int departmentId, CancellationToken cancellationToken) =>
        Task.FromResult(users?.Stored.Any(user => user.DepartmentId == departmentId) ?? false);

    public Task<IReadOnlyList<Department>> ListOrderedAsync(CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<Department>>([.. Items.OrderBy(department => department.Name)]);
}
