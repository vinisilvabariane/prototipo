using FluentResults;
using MeOne.Domain.Rules;

namespace MeOne.Application.Interfaces.Auth;

public interface ILoadAccessProfileUseCase
{
    Task<Result<AccessProfile>> ExecuteAsync(ulong userId, CancellationToken cancellationToken);
}
