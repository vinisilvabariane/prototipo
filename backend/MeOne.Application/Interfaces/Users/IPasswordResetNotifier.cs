using MeOne.Domain.Entities;

namespace MeOne.Application.Interfaces.Users;

public interface IPasswordResetNotifier
{
    Task<bool> SendResetCodeAsync(User user, string code, DateTime expiresAtUtc, CancellationToken cancellationToken);
}
