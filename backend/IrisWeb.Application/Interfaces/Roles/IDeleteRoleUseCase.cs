using FluentResults;

namespace IrisWeb.Application.Interfaces.Roles;

public interface IDeleteRoleUseCase
{
    Task<Result> ExecuteAsync(ulong id, CancellationToken cancellationToken);
}
