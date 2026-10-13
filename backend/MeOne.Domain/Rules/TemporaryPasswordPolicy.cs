using System.Security.Cryptography;

namespace MeOne.Domain.Rules;

public static class TemporaryPasswordPolicy
{
    public const string Prefix = "Multi";

    private const string Upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string Lower = "abcdefghijkmnopqrstuvwxyz";
    private const string Digits = "23456789";
    private const int RandomLength = 6;

    public static string Generate()
    {
        const string all = Upper + Lower + Digits;

        var chars = new char[RandomLength];
        chars[0] = Upper[RandomNumberGenerator.GetInt32(Upper.Length)];
        chars[1] = Lower[RandomNumberGenerator.GetInt32(Lower.Length)];
        chars[2] = Digits[RandomNumberGenerator.GetInt32(Digits.Length)];

        for (var i = 3; i < chars.Length; i++)
            chars[i] = all[RandomNumberGenerator.GetInt32(all.Length)];

        for (var i = chars.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return Prefix + new string(chars);
    }
}
