using FluentResults;
using MeOne.Application.Communication.Oee.Requests;
using MeOne.Application.Communication.Oee.Responses;

namespace MeOne.Application.Interfaces.Oee;

public interface IListOeeRunsUseCase
{
    Task<Result<OeeComparisonResponse>> ExecuteAsync(OeeRunsQuery query, CancellationToken cancellationToken);
}
