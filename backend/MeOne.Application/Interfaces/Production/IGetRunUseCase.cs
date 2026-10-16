using FluentResults;
using MeOne.Application.Communication.Production.Responses;

namespace MeOne.Application.Interfaces.Production;

public interface IGetRunUseCase
{
    Task<Result<RunDetailResponse>> ExecuteAsync(ulong id, CancellationToken cancellationToken);
}
