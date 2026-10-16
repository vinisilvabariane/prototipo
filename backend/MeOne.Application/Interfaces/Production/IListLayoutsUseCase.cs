using FluentResults;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Communication.Production.Responses;

namespace MeOne.Application.Interfaces.Production;

public interface IListLayoutsUseCase
{
    Task<Result<LayoutListResponse>> ExecuteAsync(LayoutsQuery query, CancellationToken cancellationToken);
}
