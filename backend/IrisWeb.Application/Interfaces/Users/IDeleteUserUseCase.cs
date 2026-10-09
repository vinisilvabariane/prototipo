using FluentResults;

namespace IrisWeb.Application.Interfaces.Users;

public interface IDeleteUserUseCase
{
    Task<Result> ExecuteAsync(ulong id, CancellationToken cancellationToken);
}
