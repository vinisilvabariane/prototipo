using MeOne.Domain.Rules;

namespace MeOne.Application.Interfaces;

public interface ICurrentActor
{
    string? Registration { get; }

    ulong? UserId { get; }

    Task<AccessProfile?> GetAccessProfileAsync(CancellationToken cancellationToken);
}
