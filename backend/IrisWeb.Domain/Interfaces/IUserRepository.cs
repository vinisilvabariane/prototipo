using IrisWeb.Domain.Entities;

namespace IrisWeb.Domain.Interfaces;

/// <summary>
/// Usuário desativado (soft delete) não aparece em nenhuma consulta, exceto nas checagens de
/// unicidade: o índice único de matrícula/e-mail no banco também conta os desativados.
/// </summary>
public interface IUserRepository : IRepository<User, ulong>
{
    Task<User?> GetByRegistrationAsync(uint registration, CancellationToken cancellationToken);

    /// <summary>Com roles, aplicações e departamento — o que o JWT precisa.</summary>
    Task<User?> GetWithAccessAsync(ulong id, CancellationToken cancellationToken);

    /// <summary>Com roles, aplicações e departamento — o que o JWT precisa.</summary>
    Task<User?> GetWithAccessByRegistrationAsync(uint registration, CancellationToken cancellationToken);

    Task<bool> RegistrationExistsAsync(uint registration, ulong? exceptUserId, CancellationToken cancellationToken);

    Task<bool> EmailExistsAsync(string email, ulong? exceptUserId, CancellationToken cancellationToken);

    Task<int> CountAsync(string? search, CancellationToken cancellationToken);

    Task<IReadOnlyList<User>> SearchAsync(string? search, int skip, int take, CancellationToken cancellationToken);
}
