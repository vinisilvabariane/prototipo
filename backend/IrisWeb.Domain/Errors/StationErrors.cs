namespace IrisWeb.Domain.Errors;

public static class StationErrors
{
    public const string NameRequired = "Informe o nome da estação (ex.: TABLET-EXTREMA-01).";
    public const string WorkstationRequired = "Informe o posto.";
    public const string LineRequired = "Informe a linha.";
    public const string NotConfigured = "Estação não configurada.";
    public const string RegistrationRequired = "Matrícula não informada.";
    public const string InvalidSession = "Sessão de presença inválida.";
    public const string NothingPersisted = "Nenhuma alteração foi persistida no banco de dados.";

    public static string AlreadyExists(string station) =>
        $"Já existe uma estação chamada '{station}'.";

    public static string CollidesWith(string station, string workstation, string line) =>
        $"Já existe uma estação chamada '{station}' ({workstation} / {line}). " +
        "Selecione-a na lista em vez de cadastrar outra.";

    public static string StationNotConfigured(string station) =>
        $"Estação '{station}' não está configurada.";

    public static string FieldTooLong(string field, int max) =>
        $"{field} não pode passar de {max} caracteres.";
}
