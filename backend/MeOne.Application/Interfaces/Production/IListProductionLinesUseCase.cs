using FluentResults;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Communication.Production.Responses;

namespace MeOne.Application.Interfaces.Production;

public interface IListProductionLinesUseCase
{
    Task<Result<IReadOnlyList<ProductionLineResponse>>> ExecuteAsync(SetupQuery query, CancellationToken cancellationToken);
}
