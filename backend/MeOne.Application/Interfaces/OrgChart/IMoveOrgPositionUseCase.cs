using FluentResults;
using MeOne.Application.Communication.OrgChart.Requests;

namespace MeOne.Application.Interfaces.OrgChart;

public interface IMoveOrgPositionUseCase
{
    Task<Result> ExecuteAsync(ulong id, MoveOrgPositionRequest request, CancellationToken cancellationToken);
}
