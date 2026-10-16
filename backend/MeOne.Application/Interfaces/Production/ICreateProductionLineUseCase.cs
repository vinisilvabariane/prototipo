using FluentResults;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Communication.Production.Responses;

namespace MeOne.Application.Interfaces.Production;

public interface ICreateProductionLineUseCase
{
    Task<Result<ProductionLineResponse>> ExecuteAsync(SaveProductionLineRequest request, CancellationToken cancellationToken);
}
