using FluentResults;

namespace MeOne.Application.Interfaces.Users;

public interface IDeleteUserUseCase
{
    Task<Result> ExecuteAsync(ulong id, CancellationToken cancellationToken);
}
