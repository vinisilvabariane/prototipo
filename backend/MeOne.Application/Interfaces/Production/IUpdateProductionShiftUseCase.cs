using FluentResults;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Communication.Production.Responses;

namespace MeOne.Application.Interfaces.Production;

public interface IUpdateProductionShiftUseCase
{
    Task<Result> ExecuteAsync(ulong id, SaveProductionShiftRequest request, CancellationToken cancellationToken);
}
