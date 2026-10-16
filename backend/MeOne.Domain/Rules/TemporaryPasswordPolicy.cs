using System.Security.Cryptography;

namespace MeOne.Domain.Rules;

public static class TemporaryPasswordPolicy
{
    public const string Prefix = "Multi";

    public const int RandomLength = 12;

    private const string Upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string Lower = "abcdefghijkmnopqrstuvwxyz";
    private const string Digits = "23456789";

    public static string Generate()
    {
        const string all = Upper + Lower + Digits;

        var chars = new char[RandomLength];
        chars[0] = Upper[RandomNumberGenerator.GetInt32(Upper.Length)];
        chars[1] = Lower[RandomNumberGenerator.GetInt32(Lower.Length)];
        chars[2] = Digits[RandomNumberGenerator.GetInt32(Digits.Length)];

        for (var i = 3; i < chars.Length; i++)
            chars[i] = all[RandomNumberGenerator.GetInt32(all.Length)];

        RandomNumberGenerator.Shuffle(chars.AsSpan());

        return Prefix + new string(chars);
    }
}
