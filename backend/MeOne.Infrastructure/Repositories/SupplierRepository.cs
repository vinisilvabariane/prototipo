using MeOne.Domain.Entities;
using MeOne.Domain.Enums;
using MeOne.Domain.Interfaces;
using MeOne.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace MeOne.Infrastructure.Repositories;

public sealed class SupplierRepository(MeOneDbContext context)
    : Repository<Supplier, ulong>(context), ISupplierRepository
{
    public override Task<Supplier?> GetByIdAsync(ulong id, CancellationToken cancellationToken) =>
        DbSet.Include("_tools")
            .Include("_materials")
            .Include("_selfAssessments")
            .Include("_audits")
            .Include(supplier => supplier.Documents)
            .AsSplitQuery()
            .FirstOrDefaultAsync(supplier => supplier.Id == id, cancellationToken);

    public override void Update(Supplier entity)
    {
        if (Context.Entry(entity).State == EntityState.Detached)
            DbSet.Update(entity);
    }

    public Task<bool> SapCodeExistsAsync(
        string normalizedCode,
        MultiBranch multiBranch,
        ulong? exceptId,
        CancellationToken cancellationToken) =>
        DbSet.AnyAsync(
            supplier => supplier.SapCode == normalizedCode
                && supplier.MultiBranch == multiBranch
                && (exceptId == null || supplier.Id != exceptId),
            cancellationToken);

    public async Task<IReadOnlyList<Supplier>> ListAsync(
        string? search,
        SupplierOrigin? origin,
        MultiBranch? multiBranch,
        SupplierCriticality? criticality,
        CancellationToken cancellationToken)
    {
        var query = DbSet.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();

            query = query.Where(supplier =>
                supplier.Name.ToLower().Contains(term) ||
                supplier.SapCode.ToLower().Contains(term) ||
                (supplier.CorporateName != null && supplier.CorporateName.ToLower().Contains(term)) ||
                (supplier.TaxId != null && supplier.TaxId.Contains(term)));
        }

        if (origin is { } selectedOrigin)
            query = query.Where(supplier => supplier.Origin == selectedOrigin);

        if (multiBranch is { } selectedBranch)
            query = query.Where(supplier => supplier.MultiBranch == selectedBranch);

        if (criticality is { } selectedCriticality)
            query = query.Where(supplier => supplier.Criticality == selectedCriticality);

        return await query
            .Include("_selfAssessments")
            .Include("_audits")
            .AsSplitQuery()
            .OrderBy(supplier => supplier.Name)
            .ThenBy(supplier => supplier.Id)
            .ToListAsync(cancellationToken);
    }
}
