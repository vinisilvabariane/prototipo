using FluentResults;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Communication.Production.Responses;

namespace MeOne.Application.Interfaces.Production;

public interface IUpdateProductionProductUseCase
{
    Task<Result> ExecuteAsync(ulong id, SaveProductionProductRequest request, CancellationToken cancellationToken);
}
