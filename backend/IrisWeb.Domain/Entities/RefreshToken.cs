namespace IrisWeb.Domain.Entities;

/// <summary>
/// Só o hash do refresh token é persistido; o valor cru vai uma única vez para o cliente.
/// Datas em UTC.
/// </summary>
public sealed class RefreshToken : Entity<ulong>
{
    private RefreshToken() { }

    public string TokenHash { get; private set; } = string.Empty;

    public DateTime ExpiresAt { get; private set; }

    public bool Revoked { get; private set; }

    public ulong UserId { get; private set; }

    public static RefreshToken Issue(ulong userId, string tokenHash, DateTime expiresAtUtc) => new()
    {
        UserId = userId,
        TokenHash = tokenHash,
        ExpiresAt = expiresAtUtc,
    };

    public bool IsUsable(DateTime nowUtc) => !Revoked && ExpiresAt > nowUtc;

    public void Revoke() => Revoked = true;
}
