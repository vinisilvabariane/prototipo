namespace IrisWeb.Domain.Errors;

public static class RoleErrors
{
    public const int MaxNameLength = 100;
    public const int MaxDescriptionLength = 255;

    public const string NotFound = "Role não encontrada.";
    public const string NameRequired = "Informe o nome da role.";
    public const string DescriptionRequired = "Informe a descrição da role.";
    public const string RoleIdRequired = "Informe a role.";
    public const string InUse = "Não é possível excluir: a role está atribuída a usuários.";
    public const string AlreadyAssigned = "O usuário já tem essa role.";
    public const string NotAssigned = "O usuário não tem essa role.";
    public const string MasterRoleProtected = "A role master não pode ser renomeada nem excluída.";
    public const string CannotRevokeOwnMaster = "Você não pode remover a role master de si mesmo.";
    public const string NothingPersisted = "Nenhuma alteração foi persistida no banco de dados.";

    public static string NameTaken(string name) => $"A role \"{name}\" já existe.";

    public static string FieldTooLong(string field, int max) =>
        $"{field} não pode passar de {max} caracteres.";
}
