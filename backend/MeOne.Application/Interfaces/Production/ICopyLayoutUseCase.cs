using FluentResults;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Communication.Production.Responses;

namespace MeOne.Application.Interfaces.Production;

public interface ICopyLayoutUseCase
{
    Task<Result<CopyLayoutResponse>> ExecuteAsync(ulong id, CopyLayoutRequest request, CancellationToken cancellationToken);
}
