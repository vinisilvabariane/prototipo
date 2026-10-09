using IrisWeb.Domain.Enums;
using IrisWeb.Domain.Rules;

namespace IrisWeb.Tests.Domain;

public sealed class ArgosShiftRuleTests
{
    private static DateTime At(int hour, int minute, int second = 0) => new(2026, 10, 1, hour, minute, second);

    [Theory]
    [InlineData(7, 27, 59, false)]
    [InlineData(7, 28, 0, true)]
    [InlineData(17, 27, 59, true)]
    [InlineData(17, 28, 0, false)]
    public void Adm_RunsFrom0728UpToButNotIncluding1728(int hour, int minute, int second, bool expected) =>
        Assert.Equal(expected, ArgosShiftRule.Matches(ArgosShiftFilter.Adm, At(hour, minute, second)));

    [Theory]
    [InlineData(17, 27, 59, false)]
    [InlineData(17, 28, 0, true)]
    [InlineData(23, 59, 59, true)]
    [InlineData(0, 0, 0, true)]
    [InlineData(2, 59, 59, true)]
    [InlineData(3, 0, 0, false)]
    [InlineData(7, 27, 59, false)]
    public void T2_CrossesMidnightAndEndsAt0300(int hour, int minute, int second, bool expected) =>
        Assert.Equal(expected, ArgosShiftRule.Matches(ArgosShiftFilter.T2, At(hour, minute, second)));

    [Theory]
    [InlineData(4, 0)]
    [InlineData(12, 0)]
    public void All_AlwaysMatches(int hour, int minute) =>
        Assert.True(ArgosShiftRule.Matches(ArgosShiftFilter.All, At(hour, minute)));
}
