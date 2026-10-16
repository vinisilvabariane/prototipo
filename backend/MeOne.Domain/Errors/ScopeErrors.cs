namespace MeOne.Domain.Errors;

public static class ScopeErrors
{
    public const int MaxCodeLength = 30;
    public const int MaxNameLength = 100;

    public const string NotFound = "Escopo não encontrado.";
    public const string CodeRequired = "Informe o código do escopo.";
    public const string NameRequired = "Informe o nome do escopo.";
    public const string TypeInvalid = "Tipo de escopo inválido. Use plant, area, line ou workstation.";
    public const string PlantHasNoParent = "Uma planta não tem escopo pai.";
    public const string HasChildren = "Não é possível excluir: o escopo tem escopos filhos.";
    public const string InUse = "Não é possível excluir: há papéis atribuídos neste escopo.";
    public const string NothingPersisted = "Nenhuma alteração foi persistida no banco de dados.";

    public static string CodeTaken(string code) => $"O código \"{code}\" já está em uso.";

    public static string ParentRequired(string parentType) => $"Informe o escopo pai ({parentType}).";

    public static string ParentMustBe(string parentType) => $"O escopo pai precisa ser do tipo {parentType}.";

    public static string FieldTooLong(string field, int max) =>
        $"{field} não pode passar de {max} caracteres.";
}
