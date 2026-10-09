using IrisWeb.Domain.Entities;

namespace IrisWeb.Tests.Domain;

public sealed class StationVacancyTests
{
    private static StationVacancy Open() => DeviceMatrixBinding.Create("tablet-01", "n111", "SKU", "1º TURNO", 14, "prensa bateria").OpenVacancy(0);

    [Fact]
    public void Open_StartsOpenWithTheStationLabels()
    {
        var vacancy = Open();

        Assert.True(vacancy.IsOpen);
        Assert.Equal("#14 PRENSA BATERIA", vacancy.Workstation);
        Assert.Equal("N111", vacancy.Line);
        Assert.Null(vacancy.EndedAt);
        Assert.Null(vacancy.DurationSeconds);
    }

    [Fact]
    public void Close_SetsEndedAtAndDuration()
    {
        var vacancy = Open();

        Assert.True(vacancy.Close(0));
        Assert.False(vacancy.IsOpen);
        Assert.NotNull(vacancy.EndedAt);
        Assert.NotNull(vacancy.DurationSeconds);
    }

    [Fact]
    public void Close_Twice_KeepsTheFirstResult()
    {
        var vacancy = Open();

        Assert.True(vacancy.Close(0));
        var endedAt = vacancy.EndedAt;

        Assert.False(vacancy.Close(0));
        Assert.Equal(endedAt, vacancy.EndedAt);
    }

    [Fact]
    public void Open_StartsInThePastByTheInformedInterval()
    {
        var vacancy = DeviceMatrixBinding.Create("tablet-01", "n111", "SKU", "1º TURNO", 14, "prensa bateria").OpenVacancy(30);

        Assert.True((DateTime.Now - vacancy.StartedAt).TotalSeconds >= 29);
    }

    [Fact]
    public void Close_WithIntervalLongerThanTheVacancy_ClampsToZero()
    {
        var vacancy = Open();

        Assert.True(vacancy.Close(86_400));
        Assert.Equal(vacancy.StartedAt, vacancy.EndedAt);
        Assert.Equal(0, vacancy.DurationSeconds);
    }
}
