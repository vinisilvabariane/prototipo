using FluentResults;
using MeOne.Application.Communication.Auth.Responses;

namespace MeOne.Application.Interfaces.Auth;

public interface IGetMyAccessUseCase
{
    Task<Result<MyAccessResponse>> ExecuteAsync(CancellationToken cancellationToken);
}
