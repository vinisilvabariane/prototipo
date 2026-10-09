namespace IrisWeb.Application.Interfaces.Auth;

public interface IRefreshTokenGenerator
{
    /// <summary>Valor cru, que vai para o cliente e nunca é persistido.</summary>
    string Generate();

    /// <summary>Hash determinístico do valor cru: é o que fica no banco e o que se procura no refresh.</summary>
    string Hash(string token);
}
