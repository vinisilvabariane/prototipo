using FluentResults;
using IrisWeb.Application.Communication.Stations.Requests;
using IrisWeb.Application.Interfaces.Stations;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;

namespace IrisWeb.Application.UseCases.Stations;

public sealed class ClosePresenceUseCase(IStationPresenceRepository presences) : IClosePresenceUseCase
{
    public async Task<Result> ExecuteAsync(
        long sessionId,
        ClosePresenceRequest request,
        CancellationToken cancellationToken)
    {
        if (sessionId <= 0)
            return Result.Fail(StationErrors.InvalidSession);

        var presence = await presences.GetByIdAsync(sessionId, cancellationToken);

        if (presence is null || !presence.Close(request.GraceSeconds))
            return Result.Ok();

        presences.Update(presence);
        await presences.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}
