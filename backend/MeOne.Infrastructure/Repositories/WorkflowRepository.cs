using MeOne.Domain.Entities;
using MeOne.Domain.Enums;
using MeOne.Domain.Interfaces;
using MeOne.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace MeOne.Infrastructure.Repositories;

public sealed class WorkflowRepository(MeOneDbContext context)
    : Repository<Workflow, ulong>(context), IWorkflowRepository
{
    public override Task<Workflow?> GetByIdAsync(ulong id, CancellationToken cancellationToken) =>
        DbSet.Include(workflow => workflow.Assignees)
            .Include(workflow => workflow.Steps)
            .Include(workflow => workflow.Attachments)
            .Include(workflow => workflow.Messages)
            .AsSplitQuery()
            .FirstOrDefaultAsync(workflow => workflow.Id == id, cancellationToken);

    public override void Update(Workflow entity)
    {
        if (Context.Entry(entity).State == EntityState.Detached)
            DbSet.Update(entity);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            Context.ChangeTracker.Clear();
            return 0;
        }
    }

    public Task<int> CountAsync(
        WorkflowListFilter filter,
        ulong actorId,
        string? search,
        CancellationToken cancellationToken) =>
        Filtered(filter, actorId, search).CountAsync(cancellationToken);

    public async Task<IReadOnlyList<Workflow>> SearchAsync(
        WorkflowListFilter filter,
        ulong actorId,
        string? search,
        int skip,
        int take,
        CancellationToken cancellationToken) =>
        await Filtered(filter, actorId, search)
            .OrderByDescending(workflow => workflow.CreatedAt)
            .ThenByDescending(workflow => workflow.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyDictionary<WorkflowStatus, int>> CountByStatusAsync(CancellationToken cancellationToken) =>
        await DbSet.AsNoTracking()
            .GroupBy(workflow => workflow.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToDictionaryAsync(row => row.Status, row => row.Count, cancellationToken);

    public static IQueryable<Workflow> ApplyFilter(
        IQueryable<Workflow> query,
        WorkflowListFilter filter,
        ulong actorId,
        string? search)
    {
        query = filter switch
        {
            WorkflowListFilter.Mine => query.Where(workflow => workflow.CreatorId == actorId),
            WorkflowListFilter.Participating => query.Where(
                workflow => workflow.Assignees.Any(assignee => assignee.UserId == actorId)),
            _ => query,
        };

        if (string.IsNullOrWhiteSpace(search))
            return query;

        var term = search.Trim().ToLower();

        return query.Where(workflow =>
            workflow.Title.ToLower().Contains(term) ||
            workflow.Description.ToLower().Contains(term));
    }

    private IQueryable<Workflow> Filtered(WorkflowListFilter filter, ulong actorId, string? search) =>
        ApplyFilter(DbSet.AsNoTracking(), filter, actorId, search);
}
