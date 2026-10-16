using FluentResults;
using MeOne.Application.Communication.OrgChart.Requests;

namespace MeOne.Application.Interfaces.OrgChart;

public interface IAssignOrgPositionHolderUseCase
{
    Task<Result> ExecuteAsync(ulong id, AssignOrgPositionHolderRequest request, CancellationToken cancellationToken);
}
