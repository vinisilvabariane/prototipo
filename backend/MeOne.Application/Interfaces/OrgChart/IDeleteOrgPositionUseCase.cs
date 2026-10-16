using FluentResults;

namespace MeOne.Application.Interfaces.OrgChart;

public interface IDeleteOrgPositionUseCase
{
    Task<Result> ExecuteAsync(ulong id, CancellationToken cancellationToken);
}
