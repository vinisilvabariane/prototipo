using FluentResults;

namespace MeOne.Application.Interfaces.Roles;

public interface IDeleteRoleUseCase
{
    Task<Result> ExecuteAsync(ulong id, CancellationToken cancellationToken);
}
