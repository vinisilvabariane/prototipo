using FluentResults;
using MeOne.Application.Communication.OrgChart.Responses;

namespace MeOne.Application.Interfaces.OrgChart;

public interface IListOrgChartUseCase
{
    Task<Result<OrgChartResponse>> ExecuteAsync(CancellationToken cancellationToken);
}
