using FluentResults;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Communication.Production.Responses;

namespace MeOne.Application.Interfaces.Production;

public interface IUpdateProductionLineUseCase
{
    Task<Result> ExecuteAsync(ulong id, SaveProductionLineRequest request, CancellationToken cancellationToken);
}
