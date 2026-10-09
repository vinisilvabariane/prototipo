using IrisWeb.Domain.Entities;

namespace IrisWeb.Tests.Domain;

public sealed class StationPresenceTests
{
    private static StationPresence Open() =>
        DeviceMatrixBinding.Create("tablet-01", "n111", "SKU", "1º TURNO", 14, "prensa bateria").OpenPresence("12345");

    [Fact]
    public void Open_StartsOpenWithTheStationLabels()
    {
        var presence = Open();

        Assert.True(presence.IsOpen);
        Assert.Equal("#14 PRENSA BATERIA", presence.Workstation);
        Assert.Equal("N111", presence.Line);
        Assert.Null(presence.LeftAt);
        Assert.Null(presence.DurationSeconds);
    }

    [Fact]
    public void Close_SetsLeftAtAndDuration()
    {
        var presence = Open();

        Assert.True(presence.Close(0));
        Assert.False(presence.IsOpen);
        Assert.NotNull(presence.LeftAt);
        Assert.NotNull(presence.DurationSeconds);
    }

    [Fact]
    public void Close_Twice_KeepsTheFirstResult()
    {
        var presence = Open();
        presence.Close(0);
        var first = presence.LeftAt;

        Assert.False(presence.Close(0));
        Assert.Equal(first, presence.LeftAt);
    }

    [Fact]
    public void Close_GraceLongerThanTheSession_NeverGoesNegative()
    {
        var presence = Open();

        presence.Close(3600);

        Assert.Equal(0, presence.DurationSeconds);
        Assert.Equal(presence.EnteredAt, presence.LeftAt);
    }

    [Fact]
    public void Close_NegativeGrace_IsTreatedAsZero()
    {
        var presence = Open();

        presence.Close(-30);

        Assert.NotNull(presence.DurationSeconds);
        Assert.True(presence.DurationSeconds >= 0);
    }
}
