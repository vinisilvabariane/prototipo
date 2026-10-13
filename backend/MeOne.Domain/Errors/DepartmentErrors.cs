namespace MeOne.Domain.Errors;

public static class DepartmentErrors
{
    public const int MaxNameLength = 50;

    public const string NotFound = "Departamento não encontrado.";
    public const string NameRequired = "Informe o nome do departamento.";
    public const string InUse = "Não é possível excluir: há usuários vinculados a este departamento.";
    public const string NothingPersisted = "Nenhuma alteração foi persistida no banco de dados.";

    public static string NameTaken(string name) => $"O departamento \"{name}\" já existe.";

    public static string FieldTooLong(string field, int max) =>
        $"{field} não pode passar de {max} caracteres.";
}
