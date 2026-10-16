using FluentResults;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Communication.Production.Responses;

namespace MeOne.Application.Interfaces.Production;

public interface IListProductionProductsUseCase
{
    Task<Result<IReadOnlyList<ProductionProductResponse>>> ExecuteAsync(SetupQuery query, CancellationToken cancellationToken);
}
