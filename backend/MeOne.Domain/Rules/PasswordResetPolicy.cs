using System.Security.Cryptography;

namespace MeOne.Domain.Rules;

public static class PasswordResetPolicy
{
    public const int CodeLength = 8;

    public const int MaxFailedAttempts = 5;

    public static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(15);

    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static string GenerateCode() =>
        new([.. Enumerable.Range(0, CodeLength).Select(_ => Alphabet[RandomNumberGenerator.GetInt32(Alphabet.Length)])]);

    public static string NormalizeCode(string code) => code.Trim().ToUpperInvariant();
}
