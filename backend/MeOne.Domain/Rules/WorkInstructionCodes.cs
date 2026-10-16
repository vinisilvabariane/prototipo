namespace MeOne.Domain.Rules;

public static class WorkInstructionCodes
{
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static string Generate(DateTime nowUtc, Func<int, int> nextIndex)
    {
        var suffix = new string([.. Enumerable.Range(0, 5).Select(_ => Alphabet[nextIndex(Alphabet.Length)])]);

        return $"IT-{nowUtc:yyyyMMdd}-{suffix}";
    }
}
