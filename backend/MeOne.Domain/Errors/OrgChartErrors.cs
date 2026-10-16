using MeOne.Domain.Rules;

namespace MeOne.Domain.Errors;

public static class OrgChartErrors
{
    public const int MaxTitleLength = 100;

    public const string NotFound = "Cargo não encontrado.";
    public const string ParentNotFound = "O cargo superior não foi encontrado.";
    public const string TitleRequired = "Informe o nome do cargo.";
    public const string DepartmentRequired = "Selecione o departamento do cargo.";
    public const string HolderNotFound = "O colaborador não existe ou está desativado.";
    public const string HasSubordinates =
        "Não é possível excluir: o cargo tem subordinados. Mova ou exclua os cargos abaixo dele primeiro.";
    public const string MoveUnderItself = "Um cargo não pode ficar abaixo dele mesmo nem de um cargo subordinado a ele.";
    public const string NothingPersisted = "Nenhuma alteração foi persistida no banco de dados.";

    public static string TooDeep => $"A hierarquia não pode passar de {OrgChartHierarchy.MaxDepth} níveis.";

    public static string HolderTaken(string title) => $"Este colaborador já ocupa o cargo \"{title}\".";

    public static string FieldTooLong(string field, int max) =>
        $"{field} não pode passar de {max} caracteres.";
}
