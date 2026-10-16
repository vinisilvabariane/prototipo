using FluentResults;
using MeOne.Application.Communication.Users.Responses;

namespace MeOne.Application.Interfaces.Users;

public interface IIssueTemporaryPasswordUseCase
{
    Task<Result<TemporaryPasswordResponse>> ExecuteAsync(ulong userId, CancellationToken cancellationToken);
}
