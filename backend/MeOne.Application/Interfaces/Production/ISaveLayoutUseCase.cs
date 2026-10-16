using FluentResults;
using MeOne.Application.Communication.Production.Requests;

namespace MeOne.Application.Interfaces.Production;

public interface ISaveLayoutUseCase
{
    Task<Result> ExecuteAsync(ulong id, SaveLayoutRequest request, CancellationToken cancellationToken);
}
