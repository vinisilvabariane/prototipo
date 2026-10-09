using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Interfaces;
using IrisWeb.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace IrisWeb.Infrastructure.Repositories;

public sealed class UserRepository(IrisWebDbContext context)
    : Repository<User, ulong>(context), IUserRepository
{
    public override Task<User?> GetByIdAsync(ulong id, CancellationToken cancellationToken) =>
        DbSet.Include(user => user.Department)
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    // A entidade já vem rastreada das consultas acima; Update em entidade rastreada marcaria o
    // grafo inteiro (departamento, roles) como modificado.
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

    private IQueryable<User> WithAccess() =>
        DbSet.Include(user => user.Department)
            .Include(user => user.Roles).ThenInclude(link => link.Role)
            .Include(user => user.Applications).ThenInclude(link => link.Application)
            .AsSplitQuery();

    /// <summary>Nome ou e-mail contendo o termo, ou matrícula exata quando o termo é número.</summary>
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
