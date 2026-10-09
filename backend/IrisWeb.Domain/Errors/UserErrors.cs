namespace IrisWeb.Domain.Errors;

public static class UserErrors
{
    public const uint MinRegistration = 1;
    public const uint MaxRegistration = 999_999;
    public const int MinNameLength = 10;
    public const int MaxNameLength = 255;
    public const int MaxEmailLength = 100;
    public const int MinPasswordLength = 8;

    // O BCrypt ignora tudo depois do 72º byte; aceitar mais daria a falsa impressão de senha maior.
    public const int MaxPasswordLength = 72;

    public const string CorporateEmailDomain = "@grupomultilaser.com.br";

    public const string NotFound = "Usuário não encontrado.";
    public const string RegistrationOutOfRange = "A matrícula deve estar entre 1 e 999999.";
    public const string NameRequired = "Informe o nome.";
    public const string EmailRequired = "Informe o e-mail.";
    public const string EmailInvalid = "O e-mail informado não é válido.";
    public const string EmailNotCorporate = "O e-mail deve ser do domínio grupomultilaser.";
    public const string PasswordRequired = "Informe a senha.";
    public const string DepartmentRequired = "Informe o departamento.";
    public const string DepartmentInvalid = "Departamento inválido.";
    public const string RegistrationTaken = "Já existe um usuário (ativo ou desativado) com essa matrícula.";
    public const string EmailTaken = "Já existe um usuário com esse e-mail.";
    public const string NotInCollaboratorBase = "Matrícula não encontrada na base de colaboradores.";
    public const string NoEmailForReset =
        "Usuário sem e-mail cadastrado. Peça a um master para redefinir a senha.";
    public const string ResetEmailFailed =
        "Não foi possível enviar o e-mail com a nova senha; a senha não foi alterada. Tente de novo mais tarde.";
    public const string CannotDeleteSelf = "Você não pode desativar o seu próprio usuário.";
    public const string UserIdRequired = "Informe o usuário.";
    public const string NothingPersisted = "Nenhuma alteração foi persistida no banco de dados.";

    public static string NameLength =>
        $"O nome deve ter entre {MinNameLength} e {MaxNameLength} caracteres.";

    public static string PasswordLength =>
        $"A senha deve ter entre {MinPasswordLength} e {MaxPasswordLength} caracteres.";

    public static string FieldTooLong(string field, int max) =>
        $"{field} não pode passar de {max} caracteres.";
}
