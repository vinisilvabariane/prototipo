namespace MeOne.Domain.Errors;

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
    public const string MasterRoleProtected = "O papel de administrador não pode ser renomeado, excluído nem ter as permissões alteradas.";
    public const string CannotRevokeOwnMaster = "Você não pode remover o papel de administrador de si mesmo.";
    public const string MasterMustBeGlobal = "O papel de administrador só pode ser atribuído sem escopo.";
    public const string BeyondOwnPermissions =
        "Você não pode conceder ou retirar um papel com permissões que você não tem nesse escopo.";
    public const string PermissionsBeyondOwn = "Você não pode incluir num papel permissões que você não tem.";
    public const string GlobalOnlyPermissionInScope =
        "Este papel tem permissões que só valem sem escopo (administração); atribua-o sem escopo.";
    public const string NothingPersisted = "Nenhuma alteração foi persistida no banco de dados.";

    public static string NameTaken(string name) => $"A role \"{name}\" já existe.";

    public static string UnknownPermission(string code) => $"A permissão \"{code}\" não existe.";

    public static string FieldTooLong(string field, int max) =>
        $"{field} não pode passar de {max} caracteres.";
}
