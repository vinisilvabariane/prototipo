using MeOne.Domain.Entities;

namespace MeOne.Application.Interfaces.Users;

public interface IPasswordResetNotifier
{
    Task<bool> NotifyAsync(User user, string temporaryPassword, CancellationToken cancellationToken);
}
