namespace MeOne.Domain.Errors;

public static class OeeErrors
{
    public const int MaxFormulaLength = 1000;
    public const int MaxSetNameLength = 60;
    public const int MaxCompareRuns = 60;

    public const string SetNotFound = "Conjunto de fórmulas não encontrado.";
    public const string SetNameRequired = "Informe o nome do conjunto de fórmulas.";
    public const string ActiveSetCannotBeDeleted = "O conjunto ativo não pode ser excluído. Ative outro ou volte ao padrão antes.";
    public const string RunNotStarted = "A produção ainda não foi iniciada, então não há OEE para calcular.";
    public const string PeriodInvalid = "O período precisa ter início e fim, e o fim não pode ser anterior ao início.";
    public const string PeriodTooLong = "O período pode ter no máximo 92 dias.";
    public const string ScopeInvalid = "Escopo de fórmula inválido.";
    public const string FormulaRequired = "Escreva a fórmula.";
    public const string NothingPersisted = "Nenhuma alteração foi persistida no banco de dados.";

    public static string NameTaken(string name) => $"Já existe um conjunto de fórmulas chamado \"{name}\".";

    public static string FormulaTooLong => $"A fórmula não pode passar de {MaxFormulaLength} caracteres.";

    public static string FormulaInvalid(string label, string message) => $"Fórmula de {label}: {message}";
}
