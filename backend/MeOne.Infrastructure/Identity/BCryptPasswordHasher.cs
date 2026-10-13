using BCrypt.Net;
using MeOne.Application.Interfaces.Auth;

namespace MeOne.Infrastructure.Identity;

public sealed class BCryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    private static readonly Lazy<string> DecoyHash =
        new(() => BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N"), WorkFactor));

    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string? hash)
    {
        if (string.IsNullOrEmpty(hash))
        {
            BCrypt.Net.BCrypt.Verify(password, DecoyHash.Value);
            return false;
        }

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch (SaltParseException)
        {
            return false;
        }
    }
}
