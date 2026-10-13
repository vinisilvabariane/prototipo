using MeOne.Domain.Entities;

namespace MeOne.Application.Interfaces.Auth;

public interface IAccessTokenIssuer
{
    string Issue(User user);
}
