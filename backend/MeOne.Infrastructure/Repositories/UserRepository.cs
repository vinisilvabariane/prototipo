using MeOne.Domain.Entities;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;
using MeOne.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace MeOne.Infrastructure.Repositories;

public sealed class UserRepository(MeOneDbContext context)
    : Repository<User, ulong>(context), IUserRepository
{
    public override Task<User?> GetByIdAsync(ulong id, CancellationToken cancellationToken) =>
        DbSet.Include(user => user.Department)
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    public override void Update(User entity)
    {
        if (Context.Entry(entity).State == EntityState.Detached)
            DbSet.Update(entity);
    }

    public Task<User?> GetByRegistrationAsync(uint registration, CancellationToken cancellationToken) =>
        DbSet.Include(user => user.Department)
            .FirstOrDefaultAsync(user => user.Registration == registration, cancellationToken);

    public Task<User?> GetWithAccessAsync(ulong id, CancellationToken cancellationToken) =>
        WithAccess().FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    public Task<User?> GetWithAccessByRegistrationAsync(uint registration, CancellationToken cancellationToken) =>
        WithAccess().FirstOrDefaultAsync(user => user.Registration == registration, cancellationToken);

    public Task<bool> RegistrationExistsAsync(
        uint registration,
        ulong? exceptUserId,
        CancellationToken cancellationToken) =>
        DbSet.IgnoreQueryFilters()
            .AnyAsync(
                user => user.Registration == registration && (exceptUserId == null || user.Id != exceptUserId),
                cancellationToken);

    public Task<bool> EmailExistsAsync(string email, ulong? exceptUserId, CancellationToken cancellationToken) =>
        DbSet.IgnoreQueryFilters()
            .AnyAsync(
                user => user.Email == email && (exceptUserId == null || user.Id != exceptUserId),
                cancellationToken);

    public Task<int> CountAsync(string? search, CancellationToken cancellationToken) =>
        ApplySearch(DbSet.AsNoTracking(), search).CountAsync(cancellationToken);

    public async Task<IReadOnlyList<User>> SearchAsync(
        string? search,
        int skip,
        int take,
        CancellationToken cancellationToken) =>
        await ApplySearch(DbSet.AsNoTracking(), search)
            .Include(user => user.Department)
            .OrderBy(user => user.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task<int> CountActiveAdministratorsAsync(CancellationToken cancellationToken) =>
        DbSet.CountAsync(
            user => user.Roles.Any(link => link.ScopeId == null && link.Role!.Name == AccessRoles.Master),
            cancellationToken);

    public async Task<IReadOnlyList<User>> ListActiveAsync(CancellationToken cancellationToken) =>
        await DbSet.AsNoTracking()
            .Include(user => user.Department)
            .OrderBy(user => user.Name)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<User>> GetActiveByIdsAsync(
        IReadOnlyCollection<ulong> ids,
        CancellationToken cancellationToken) =>
        await DbSet.AsNoTracking()
            .Where(user => ids.Contains(user.Id))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<User>> GetByIdsIncludingInactiveAsync(
        IReadOnlyCollection<ulong> ids,
        CancellationToken cancellationToken) =>
        await DbSet.IgnoreQueryFilters()
            .AsNoTracking()
            .Where(user => ids.Contains(user.Id))
            .ToListAsync(cancellationToken);

    private IQueryable<User> WithAccess() =>
        DbSet.Include(user => user.Department)
            .Include(user => user.Roles).ThenInclude(link => link.Role!).ThenInclude(role => role.Permissions)
            .Include(user => user.Roles).ThenInclude(link => link.Scope)
            .Include(user => user.Applications).ThenInclude(link => link.Application)
            .AsSplitQuery();

    public static IQueryable<User> ApplySearch(IQueryable<User> query, string? search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return query;

        var term = search.Trim().ToLower();
        uint? registration = uint.TryParse(term, out var parsed) ? parsed : null;

        return query.Where(user =>
            user.Name.ToLower().Contains(term) ||
            (user.Email != null && user.Email.ToLower().Contains(term)) ||
            (registration != null && user.Registration == registration));
    }
}
