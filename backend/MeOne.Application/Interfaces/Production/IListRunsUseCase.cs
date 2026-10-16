using FluentResults;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Communication.Production.Responses;

namespace MeOne.Application.Interfaces.Production;

public interface IListRunsUseCase
{
    Task<Result<RunPageResponse>> ExecuteAsync(RunsQuery query, CancellationToken cancellationToken);
}
