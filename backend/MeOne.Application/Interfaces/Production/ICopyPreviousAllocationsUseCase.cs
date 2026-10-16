using FluentResults;
using MeOne.Application.Communication.Production.Responses;

namespace MeOne.Application.Interfaces.Production;

public interface ICopyPreviousAllocationsUseCase
{
    Task<Result<CopyAllocationsResponse>> ExecuteAsync(ulong runId, CancellationToken cancellationToken);
}
