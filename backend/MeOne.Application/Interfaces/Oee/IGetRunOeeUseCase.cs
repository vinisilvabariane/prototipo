using FluentResults;
using MeOne.Application.Communication.Oee.Requests;
using MeOne.Application.Communication.Oee.Responses;

namespace MeOne.Application.Interfaces.Oee;

public interface IGetRunOeeUseCase
{
    Task<Result<RunOeeResponse>> ExecuteAsync(ulong id, CancellationToken cancellationToken);
}
