using IrisWeb.Domain.Rules;

namespace IrisWeb.Tests.Domain;

public sealed class StationNamingPolicyTests
{
    [Theory]
    [InlineData("  posto 1  ", "POSTO 1")]
    [InlineData("Posto\t\t1", "POSTO 1")]
    [InlineData("montagem   final", "MONTAGEM FINAL")]
    [InlineData("", StationNamingPolicy.Default)]
    [InlineData("   ", StationNamingPolicy.Default)]
    [InlineData(null, StationNamingPolicy.Default)]
    public void Normalize_TrimsCollapsesAndUppercases(string? raw, string expected)
    {
        Assert.Equal(expected, StationNamingPolicy.Normalize(raw));
    }

    [Fact]
    public void NormalizeField_CutsAtThePresenceColumnWidth()
    {
        var normalized = StationNamingPolicy.NormalizeField("#14 " + new string('a', 100));

        Assert.Equal(StationNamingPolicy.MaxFieldLength, normalized.Length);
        Assert.StartsWith("#14 AAA", normalized);
    }
}
