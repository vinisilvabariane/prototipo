using IrisWeb.Application.Communication.Stations.Requests;
using IrisWeb.Application.UseCases.Stations;
using IrisWeb.Domain.Entities;
using IrisWeb.Tests.Fakes;

namespace IrisWeb.Tests.Application;

public sealed class StationUseCaseTests
{
    [Fact]
    public async Task ClosePresenceAsync_ClosesTheOpenSession()
    {
        var presences = new FakeStationPresenceRepository();
        var presence = DeviceMatrixBinding.Create("tablet-01", "N111", "SKU", "1º TURNO", 14, "PRENSA").OpenPresence("12345");
        await presences.AddAsync(presence, CancellationToken.None);
        await presences.SaveChangesAsync(CancellationToken.None);
        var close = new ClosePresenceUseCase(presences);

        var result = await close.ExecuteAsync(presence.Id, new ClosePresenceRequest(0), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(presence.IsOpen);
        Assert.NotNull(presence.DurationSeconds);
    }

    [Fact]
    public async Task ClosePresenceAsync_AlreadyClosed_StillSucceeds()
    {
        var presences = new FakeStationPresenceRepository();
        var useCase = new ClosePresenceUseCase(presences);

        var result = await useCase.ExecuteAsync(7, new ClosePresenceRequest(30), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ClosePresenceAsync_InvalidSession_Fails()
    {
        var useCase = new ClosePresenceUseCase(new FakeStationPresenceRepository());

        var result = await useCase.ExecuteAsync(0, new ClosePresenceRequest(0), CancellationToken.None);

        Assert.True(result.IsFailed);
    }
}
