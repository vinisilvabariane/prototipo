namespace MeOne.Application.Interfaces.Auth;

public interface IRefreshTokenGenerator
{
    string Generate();

    string Hash(string token);
}
