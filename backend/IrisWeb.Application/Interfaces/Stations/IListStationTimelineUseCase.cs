using FluentResults;
using IrisWeb.Application.Communication.Stations.Requests;
using IrisWeb.Application.Communication.Stations.Responses;

namespace IrisWeb.Application.Interfaces.Stations;

public interface IListStationTimelineUseCase
{
    Task<Result<StationTimelineResponse>> ExecuteAsync(
        StationTimelineQuery query,
        CancellationToken cancellationToken);
}
