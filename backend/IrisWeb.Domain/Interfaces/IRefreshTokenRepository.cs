using IrisWeb.Domain.Entities;

namespace IrisWeb.Domain.Interfaces;

public interface IRefreshTokenRepository : IRepository<RefreshToken, ulong>
{
    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken);

    /// <summary>Revoga direto no banco (não espera SaveChanges) e devolve quantos foram revogados.</summary>
    Task<int> RevokeAllForUserAsync(ulong userId, CancellationToken cancellationToken);
}
