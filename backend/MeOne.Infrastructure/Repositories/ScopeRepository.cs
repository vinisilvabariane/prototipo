using MeOne.Domain.Entities;
using MeOne.Domain.Interfaces;
using MeOne.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace MeOne.Infrastructure.Repositories;

public sealed class ScopeRepository(MeOneDbContext context)
    : Repository<Scope, ulong>(context), IScopeRepository
{
    private const int MaxDepth = 4;

    public override void Update(Scope entity)
    {
        if (Context.Entry(entity).State == EntityState.Detached)
            DbSet.Update(entity);
    }

    public Task<bool> CodeExistsAsync(string normalizedCode, ulong? exceptScopeId, CancellationToken cancellationToken) =>
        DbSet.AnyAsync(
            scope => scope.Code == normalizedCode && (exceptScopeId == null || scope.Id != exceptScopeId),
            cancellationToken);

    public Task<bool> HasChildrenAsync(ulong scopeId, CancellationToken cancellationToken) =>
        DbSet.AnyAsync(scope => scope.ParentId == scopeId, cancellationToken);

    public Task<bool> InUseAsync(ulong scopeId, CancellationToken cancellationToken) =>
        Context.Set<UserRole>().AnyAsync(link => link.ScopeId == scopeId, cancellationToken);

    public async Task<IReadOnlyList<ulong>> GetChainAsync(ulong scopeId, CancellationToken cancellationToken)
    {
        var chain = new List<ulong>();
        ulong? current = scopeId;

        while (current is { } id && chain.Count < MaxDepth)
        {
            var parentId = await DbSet.AsNoTracking()
                .Where(scope => scope.Id == id)
                .Select(scope => new { scope.ParentId })
                .FirstOrDefaultAsync(cancellationToken);

            if (parentId is null)
                break;

            chain.Add(id);
            current = parentId.ParentId;
        }

        return chain;
    }

    public async Task<IReadOnlyList<Scope>> ListOrderedAsync(CancellationToken cancellationToken) =>
        await DbSet.AsNoTracking()
            .OrderBy(scope => scope.Type)
            .ThenBy(scope => scope.Code)
            .ToListAsync(cancellationToken);
}
