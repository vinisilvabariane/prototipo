using FluentResults;
using IrisWeb.Application.Communication.Stations.Requests;

namespace IrisWeb.Application.Interfaces.Stations;

public interface IClosePresenceUseCase
{
    Task<Result> ExecuteAsync(
        long sessionId,
        ClosePresenceRequest request,
        CancellationToken cancellationToken);
}
