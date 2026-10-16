using MeOne.Domain.Rules;

namespace MeOne.Domain.Entities;

public sealed class PasswordResetToken : Entity<ulong>
{
    private PasswordResetToken() { }

    public ulong UserId { get; private set; }

    public string CodeHash { get; private set; } = string.Empty;

    public DateTime ExpiresAt { get; private set; }

    public DateTime? UsedAt { get; private set; }

    public int FailedAttempts { get; private set; }

    public static PasswordResetToken Issue(ulong userId, string codeHash, DateTime expiresAtUtc) => new()
    {
        UserId = userId,
        CodeHash = codeHash,
        ExpiresAt = expiresAtUtc,
    };

    public bool IsUsable(DateTime nowUtc) =>
        UsedAt is null && ExpiresAt > nowUtc && FailedAttempts < PasswordResetPolicy.MaxFailedAttempts;

    public void RegisterFailedAttempt() => FailedAttempts++;

    public void MarkUsed(DateTime nowUtc) => UsedAt = nowUtc;
}
