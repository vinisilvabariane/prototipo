using FluentResults;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Communication.Production.Responses;

namespace MeOne.Application.Interfaces.Production;

public interface ICreateProductionProductUseCase
{
    Task<Result<ProductionProductResponse>> ExecuteAsync(SaveProductionProductRequest request, CancellationToken cancellationToken);
}
