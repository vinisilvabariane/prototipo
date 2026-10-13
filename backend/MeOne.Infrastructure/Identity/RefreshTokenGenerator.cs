using System.Security.Cryptography;
using System.Text;
using MeOne.Application.Interfaces.Auth;

namespace MeOne.Infrastructure.Identity;

public sealed class RefreshTokenGenerator : IRefreshTokenGenerator
{
    private const int TokenBytes = 64;

    public string Generate() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(TokenBytes));

    public string Hash(string token) => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
