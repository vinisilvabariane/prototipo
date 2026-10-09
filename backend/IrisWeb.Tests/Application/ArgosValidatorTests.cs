using IrisWeb.Application.Communication.Argos.Requests;
using IrisWeb.Application.Validators.Argos;

namespace IrisWeb.Tests.Application;

public sealed class ArgosValidatorTests
{
    private static readonly DateTime From = new(2026, 10, 1);

    private readonly ArgosSummaryQueryValidator _summary = new();
    private readonly ArgosCyclesQueryValidator _cycles = new();

    [Fact]
    public void Summary_ValidWindow_Passes() =>
        Assert.True(_summary.Validate(new ArgosSummaryQuery { From = From, To = From.AddDays(1) }).IsValid);

    [Fact]
    public void Summary_MissingBounds_Fails()
    {
        Assert.False(_summary.Validate(new ArgosSummaryQuery { To = From }).IsValid);
        Assert.False(_summary.Validate(new ArgosSummaryQuery { From = From }).IsValid);
    }

    [Fact]
    public void Summary_EndNotAfterStart_Fails() =>
        Assert.False(_summary.Validate(new ArgosSummaryQuery { From = From, To = From }).IsValid);

    [Fact]
    public void Summary_31DaysPasses_33DaysFails()
    {
        Assert.True(_summary.Validate(new ArgosSummaryQuery { From = From, To = From.AddDays(31) }).IsValid);
        Assert.False(_summary.Validate(new ArgosSummaryQuery { From = From, To = From.AddDays(33) }).IsValid);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(500, true)]
    [InlineData(501, false)]
    public void Cycles_PageSizeGoesFrom1To500(int pageSize, bool valid) =>
        Assert.Equal(
            valid,
            _cycles.Validate(new ArgosCyclesQuery { From = From, To = From.AddDays(1), PageSize = pageSize }).IsValid);

    [Fact]
    public void Cycles_PageStartsAtOne() =>
        Assert.False(_cycles.Validate(new ArgosCyclesQuery { From = From, To = From.AddDays(1), Page = 0 }).IsValid);
}
