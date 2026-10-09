using FluentResults;
using IrisWeb.Application.Communication.Stations.Requests;
using IrisWeb.Application.Communication.Stations.Responses;

namespace IrisWeb.Application.Interfaces.Stations;

public interface IGetLineTimelineUseCase
{
    Task<Result<LineTimelineResponse>> ExecuteAsync(LineTimelineQuery query, CancellationToken cancellationToken);
}
