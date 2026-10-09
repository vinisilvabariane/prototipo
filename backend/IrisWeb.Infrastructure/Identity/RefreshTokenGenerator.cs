using System.Security.Cryptography;
using System.Text;
using IrisWeb.Application.Interfaces.Auth;

namespace IrisWeb.Infrastructure.Identity;

/// <summary>
/// 64 bytes aleatórios; no banco vai o SHA-256 em Base64 (sem sal: o valor já é aleatório e a busca
/// precisa ser determinística).
/// </summary>
public sealed class RefreshTokenGenerator : IRefreshTokenGenerator
{
    private const int TokenBytes = 64;

    public string Generate() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(TokenBytes));

    public string Hash(string token) => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
