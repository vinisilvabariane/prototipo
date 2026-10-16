namespace MeOne.Domain.Rules;

public static class Cnpj
{
    private static readonly int[] FirstWeights = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

    private static readonly int[] SecondWeights = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

    public static string Digits(string value) => new([.. value.Where(char.IsAsciiDigit)]);

    public static bool IsValid(string value)
    {
        var digits = Digits(value);

        if (digits.Length != 14 || digits.Distinct().Count() == 1)
            return false;

        var first = CheckDigit(digits[..12], FirstWeights);
        var second = CheckDigit(digits[..12] + first, SecondWeights);

        return digits[12] == first && digits[13] == second;
    }

    private static char CheckDigit(string digits, int[] weights)
    {
        var sum = digits.Select((digit, index) => (digit - '0') * weights[index]).Sum();
        var remainder = sum % 11;

        return (char)('0' + (remainder < 2 ? 0 : 11 - remainder));
    }
}
