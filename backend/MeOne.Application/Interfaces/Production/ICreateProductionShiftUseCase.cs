using FluentResults;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Communication.Production.Responses;

namespace MeOne.Application.Interfaces.Production;

public interface ICreateProductionShiftUseCase
{
    Task<Result<ProductionShiftResponse>> ExecuteAsync(SaveProductionShiftRequest request, CancellationToken cancellationToken);
}
