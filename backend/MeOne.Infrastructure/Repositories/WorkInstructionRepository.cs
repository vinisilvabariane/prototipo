using MeOne.Domain.Entities;
using MeOne.Domain.Interfaces;
using MeOne.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace MeOne.Infrastructure.Repositories;

public sealed class WorkInstructionRepository(MeOneDbContext context)
    : Repository<WorkInstruction, ulong>(context), IWorkInstructionRepository
{
    public override Task<WorkInstruction?> GetByIdAsync(ulong id, CancellationToken cancellationToken) =>
        DbSet.Include(instruction => instruction.Steps)
            .FirstOrDefaultAsync(instruction => instruction.Id == id, cancellationToken);

    public override void Update(WorkInstruction entity)
    {
        if (Context.Entry(entity).State == EntityState.Detached)
            DbSet.Update(entity);
    }

    public Task<bool> CodeExistsAsync(string normalizedCode, ulong? exceptId, CancellationToken cancellationToken) =>
        DbSet.AnyAsync(
            instruction => instruction.Code == normalizedCode && (exceptId == null || instruction.Id != exceptId),
            cancellationToken);

    public Task<int> CountAsync(string? search, CancellationToken cancellationToken) =>
        ApplySearch(DbSet.AsNoTracking(), search).CountAsync(cancellationToken);

    public async Task<IReadOnlyList<WorkInstruction>> SearchAsync(
        string? search,
        int skip,
        int take,
        CancellationToken cancellationToken) =>
        await ApplySearch(DbSet.AsNoTracking(), search)
            .Include(instruction => instruction.Steps)
            .OrderBy(instruction => instruction.Title)
            .ThenBy(instruction => instruction.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public static IQueryable<WorkInstruction> ApplySearch(IQueryable<WorkInstruction> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return query;

        var term = search.Trim().ToLower();

        return query.Where(instruction =>
            instruction.Title.ToLower().Contains(term) ||
            instruction.Code.ToLower().Contains(term) ||
            instruction.Product.ToLower().Contains(term) ||
            instruction.DocumentNumber.ToLower().Contains(term));
    }
}
