using FluentResults;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Communication.Production.Responses;

namespace MeOne.Application.Interfaces.Production;

public interface IListProductionShiftsUseCase
{
    Task<Result<IReadOnlyList<ProductionShiftResponse>>> ExecuteAsync(SetupQuery query, CancellationToken cancellationToken);
}
