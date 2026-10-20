using FluentResults;
using MeOne.Application.Communication.Oee.Requests;
using MeOne.Application.Communication.Oee.Responses;

namespace MeOne.Application.Interfaces.Oee;

public interface IGetOeeConfigUseCase
{
    Task<Result<OeeConfigResponse>> ExecuteAsync(CancellationToken cancellationToken);
}
