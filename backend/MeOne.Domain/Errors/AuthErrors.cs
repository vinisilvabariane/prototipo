namespace MeOne.Domain.Errors;

public static class AuthErrors
{
    public const int MaxLoginPasswordLength = 100;
    public const int MaxRefreshTokenLength = 512;

    public const string InvalidCredentials = "Matrícula ou senha incorreta.";
    public const string InvalidRefreshToken = "Sessão inválida ou expirada. Faça login novamente.";
    public const string RefreshTokenRequired = "Informe o refresh token.";
    public const string PasswordRequired = "Informe a senha.";
    public const string SessionWithoutUser = "Não foi possível identificar o usuário da sessão. Faça login novamente.";
    public const string LogoutOtherUserForbidden = "Só um master pode encerrar a sessão de outro usuário.";
    public const string NothingPersisted = "Nenhuma alteração foi persistida no banco de dados.";

    public static string FieldTooLong(string field, int max) =>
        $"{field} não pode passar de {max} caracteres.";
}
