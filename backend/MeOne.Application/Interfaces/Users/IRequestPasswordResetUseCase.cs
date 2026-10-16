using FluentResults;
using MeOne.Application.Communication.Users.Requests;

namespace MeOne.Application.Interfaces.Users;

public interface IRequestPasswordResetUseCase
{
    Task<Result> ExecuteAsync(RequestPasswordResetRequest request, CancellationToken cancellationToken);
}
