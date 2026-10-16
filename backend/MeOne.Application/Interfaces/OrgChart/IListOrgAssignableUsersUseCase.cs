using FluentResults;
using MeOne.Application.Communication.OrgChart.Responses;

namespace MeOne.Application.Interfaces.OrgChart;

public interface IListOrgAssignableUsersUseCase
{
    Task<Result<IReadOnlyList<OrgUserResponse>>> ExecuteAsync(CancellationToken cancellationToken);
}
