using MeOne.Domain.Entities;
using MeOne.Domain.Enums;
using MeOne.Domain.Interfaces;
using MeOne.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace MeOne.Infrastructure.Repositories;

public sealed class HelpdeskTicketRepository(MeOneDbContext context)
    : Repository<HelpdeskTicket, ulong>(context), IHelpdeskTicketRepository
{
    public override void Update(HelpdeskTicket entity)
    {
        if (Context.Entry(entity).State == EntityState.Detached)
            DbSet.Update(entity);
    }

    public Task<int> CountAsync(HelpdeskTicketFilter filter, CancellationToken cancellationToken) =>
        Filtered(filter).CountAsync(cancellationToken);

    public async Task<IReadOnlyDictionary<HelpdeskTicketStatus, int>> CountByStatusAsync(HelpdeskTicketFilter filter, CancellationToken cancellationToken)
    {
        var rows = await Filtered(filter)
            .GroupBy(ticket => ticket.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(row => row.Status, row => row.Count);
    }

    public async Task<IReadOnlyList<HelpdeskTicket>> SearchAsync(HelpdeskTicketFilter filter, int skip, int take, CancellationToken cancellationToken) =>
        await Filtered(filter)
            .OrderByDescending(ticket => ticket.OpenedAt)
            .ThenByDescending(ticket => ticket.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<HelpdeskTicket>> ListLineLossesAsync(ulong lineId, DateTime from, DateTime to, CancellationToken cancellationToken) =>
        await DbSet.AsNoTracking()
            .Where(ticket => ticket.LineId == lineId && ticket.Status != HelpdeskTicketStatus.Rejected)
            .Where(ticket =>
                (ticket.IsLineStopped && ticket.StoppedAt != null && ticket.StoppedAt <= to && (ticket.FinishedAt == null || ticket.FinishedAt >= from))
                || (ticket.Defects > 0 && ticket.OpenedAt >= from && ticket.OpenedAt <= to))
            .ToListAsync(cancellationToken);

    public Task<bool> SectorHasTicketsAsync(ulong sectorId, CancellationToken cancellationToken) =>
        DbSet.AnyAsync(ticket => ticket.SectorId == sectorId, cancellationToken);

    private IQueryable<HelpdeskTicket> Filtered(HelpdeskTicketFilter filter)
    {
        var sectorIds = filter.SectorIds?.ToList();

        return DbSet.AsNoTracking()
            .Where(ticket => sectorIds == null || sectorIds.Contains(ticket.SectorId))
            .Where(ticket => filter.OpenedBy == null || ticket.OpenedBy == filter.OpenedBy)
            .Where(ticket => filter.Status == null || ticket.Status == filter.Status)
            .Where(ticket => filter.SectorId == null || ticket.SectorId == filter.SectorId)
            .Where(ticket => filter.LineId == null || ticket.LineId == filter.LineId)
            .Where(ticket => filter.From == null || ticket.OpenedAt >= filter.From)
            .Where(ticket => filter.To == null || ticket.OpenedAt <= filter.To);
    }
}
