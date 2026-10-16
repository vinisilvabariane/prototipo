using FluentResults;
using MeOne.Application.Communication.OrgChart.Requests;

namespace MeOne.Application.Interfaces.OrgChart;

public interface IUpdateOrgPositionUseCase
{
    Task<Result> ExecuteAsync(ulong id, UpdateOrgPositionRequest request, CancellationToken cancellationToken);
}
