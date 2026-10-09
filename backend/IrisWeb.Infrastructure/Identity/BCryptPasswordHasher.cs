using BCrypt.Net;
using IrisWeb.Application.Interfaces.Auth;

namespace IrisWeb.Infrastructure.Identity;

/// <summary>
/// BCrypt com custo 12 — o mesmo do Access Control, então as senhas da tabela <c>users</c>
/// continuam valendo.
/// </summary>
public sealed class BCryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    // Hash de uma senha qualquer, só para a matrícula inexistente gastar o mesmo tempo.
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
