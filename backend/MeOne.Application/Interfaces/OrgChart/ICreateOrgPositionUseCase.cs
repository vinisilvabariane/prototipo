using FluentResults;
using MeOne.Application.Communication.OrgChart.Requests;
using MeOne.Application.Communication.OrgChart.Responses;

namespace MeOne.Application.Interfaces.OrgChart;

public interface ICreateOrgPositionUseCase
{
    Task<Result<OrgPositionResponse>> ExecuteAsync(CreateOrgPositionRequest request, CancellationToken cancellationToken);
}
