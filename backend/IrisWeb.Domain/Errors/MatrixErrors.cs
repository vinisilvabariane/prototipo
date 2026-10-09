namespace IrisWeb.Domain.Errors;

public static class MatrixErrors
{
    public const int MaxDeviceIdLength = 64;
    public const int MaxLineLength = 100;
    public const int MaxProductLength = 50;
    public const int MaxShiftLength = 50;
    public const int MaxStationNameLength = 150;

    public const string DeviceRequired = "Identificador do aparelho não informado.";
    public const string DeviceNotBound = "Este aparelho não está vinculado a um posto da Matriz de Capacitação.";
    public const string LineRequired = "Informe a linha.";
    public const string ProductRequired = "Informe o produto.";
    public const string ShiftRequired = "Informe o turno.";
    public const string StationRequired = "Informe o posto.";
    public const string NothingPersisted = "Nenhuma alteração foi persistida no banco de dados.";
    public const string RegistrationRequired = "Matrícula não informada.";
    public const string RegistrationNotNumeric = "A matrícula reconhecida não é numérica; o Sync só aceita matrícula numérica.";

    public static string StationWithoutActivity(int stationId) =>
        $"O posto #{stationId} não tem atividade definida no layout do Sync. Peça à engenharia para configurar antes de alocar.";

    public static string StationNotInLayout(int stationId, string line, string product, string shift) =>
        $"O posto #{stationId} não existe no layout de {line} · {product} · {shift}. " +
        "O layout pode ter sido editado no Sync; vincule o aparelho de novo.";

    public static string NoFaceEnrollment(int registration) =>
        $"A matrícula {registration} não tem cadastro facial no Iris. Cadastre o rosto (Monitoramento → Cadastrar rosto) " +
        "antes de alocar; sem ele o tablet do posto não reconhece o operador.";

    public static string StationAlreadyEmpty(int stationId) =>
        $"O posto #{stationId} já está livre no Sync.";

    public static string ShiftNotFound(string shift) =>
        $"O turno \"{shift}\" não está na lista de turnos do Sync.";

    public static string FieldTooLong(string field, int max) =>
        $"{field} não pode passar de {max} caracteres.";
}
