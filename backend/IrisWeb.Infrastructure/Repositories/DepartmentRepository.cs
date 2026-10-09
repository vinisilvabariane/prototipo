using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Interfaces;
using IrisWeb.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace IrisWeb.Infrastructure.Repositories;

public sealed class DepartmentRepository(IrisWebDbContext context)
    : Repository<Department, int>(context), IDepartmentRepository
{
    public override void Update(Department entity)
    {
        if (Context.Entry(entity).State == EntityState.Detached)
            DbSet.Update(entity);
    }

    // Rastreado de propósito: o use case de usuário pendura o departamento na entidade nova, e um
    // departamento não rastreado viraria INSERT.
    public Task<Department?> GetByNameAsync(string normalizedName, CancellationToken cancellationToken) =>
        DbSet.FirstOrDefaultAsync(department => department.Name == normalizedName, cancellationToken);

    public Task<bool> NameExistsAsync(
        string normalizedName,
        int? exceptDepartmentId,
        CancellationToken cancellationToken) =>
        DbSet.AnyAsync(
            department => department.Name == normalizedName &&
                          (exceptDepartmentId == null || department.Id != exceptDepartmentId),
            cancellationToken);

    public Task<bool> InUseAsync(int departmentId, CancellationToken cancellationToken) =>
        Context.Set<User>()
            .IgnoreQueryFilters()
            .AnyAsync(user => user.DepartmentId == departmentId, cancellationToken);

    public async Task<IReadOnlyList<Department>> ListOrderedAsync(CancellationToken cancellationToken) =>
        await DbSet.AsNoTracking()
            .OrderBy(department => department.Name)
            .ToListAsync(cancellationToken);
}
