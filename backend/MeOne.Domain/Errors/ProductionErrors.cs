namespace MeOne.Domain.Errors;

public static class ProductionErrors
{
    public const int MaxLineNameLength = 60;
    public const int MaxShiftNameLength = 40;
    public const int MaxProductCodeLength = 30;
    public const int MaxProductNameLength = 120;
    public const int MaxPostNameLength = 60;
    public const int MaxOperationLength = 200;
    public const int MaxLabelLength = 60;
    public const int MaxPageSize = 100;

    public const string LineNotFound = "Linha não encontrada.";
    public const string ShiftNotFound = "Turno não encontrado.";
    public const string ProductNotFound = "Produto não encontrado.";
    public const string LayoutNotFound = "Layout não encontrado.";
    public const string RunNotFound = "Produção não encontrada.";
    public const string UserNotFound = "A pessoa não existe ou está desativada.";

    public const string LineNameRequired = "Informe o nome da linha.";
    public const string ShiftNameRequired = "Informe o nome do turno.";
    public const string ShiftTimesEqual = "O início e o fim do turno não podem ser iguais.";
    public const string ProductCodeRequired = "Informe o código do produto.";
    public const string ProductNameRequired = "Informe o nome do produto.";

    public const string LineInactive = "A linha está desativada.";
    public const string ShiftInactive = "O turno está desativado.";
    public const string ProductInactive = "O produto está desativado.";

    public const string PostNameRequired = "Todo posto precisa de um nome.";
    public const string PostKindInvalid = "Tipo de posto inválido.";
    public const string PostGeometryInvalid = "A posição ou o tamanho de um posto é inválido.";
    public const string DecorationKindInvalid = "Tipo de decoração inválido.";
    public const string DecorationGeometryInvalid = "A posição ou o tamanho de uma decoração é inválido.";
    public const string RotationInvalid = "A rotação precisa estar entre 0 e 359 graus.";
    public const string TargetInvalid = "A meta por hora precisa estar entre 0 e 100.000.";
    public const string PostRepeated = "O mesmo posto foi enviado duas vezes.";
    public const string DecorationRepeated = "A mesma decoração foi enviada duas vezes.";
    public const string PostNotInLayout = "Um dos postos enviados não pertence a este layout.";
    public const string DecorationNotInLayout = "Uma das decorações enviadas não pertence a este layout.";
    public const string TooManyPosts = "O layout pode ter no máximo 200 postos.";
    public const string TooManyDecorations = "O layout pode ter no máximo 200 decorações.";

    public const string CopyTargetsRequired = "Selecione ao menos um destino para a cópia.";
    public const string CopyTargetsTooMany = "Selecione no máximo 50 destinos por cópia.";
    public const string CopyToItself = "O destino da cópia não pode ser o próprio layout.";
    public const string CopyTargetRepeated = "Há destinos repetidos na cópia.";

    public const string RunClosed = "A produção já foi encerrada.";
    public const string LayoutNotInRun = "O layout desta produção não existe mais.";
    public const string PostNotInRun = "O posto não existe nesta produção.";
    public const string PersonHoldsCommonPost = "A pessoa está em um posto comum e não pode acumular máquinas.";
    public const string PersonAlreadyAllocated = "A pessoa já está em outro posto desta produção. Posto comum aceita apenas quem não está em nenhum outro.";
    public const string RunNotPreparing = "A produção já foi iniciada ou encerrada.";
    public const string RunNotRunning = "A produção não está em andamento.";
    public const string RunWithoutAllocations = "Aloque ao menos uma pessoa antes de iniciar a produção.";
    public const string RunCannotBeCancelled = "Só dá para cancelar uma produção que ainda não foi iniciada.";
    public const string QuantityInvalid = "A quantidade precisa ser um número inteiro entre 0 e 1.000.000.";
    public const string OutputInTheFuture = "Não é possível lançar produção de uma hora que ainda não começou.";
    public const string OutputBeforeStart = "Não é possível lançar produção de uma hora anterior ao início da produção.";
    public const string NoPreviousRun = "Não há uma produção anterior encerrada para este layout.";
    public const string NothingPersisted = "Nenhuma alteração foi persistida no banco de dados.";

    public static string NameTaken(string name) => $"Já existe um cadastro com o nome \"{name}\".";

    public static string CodeTaken(string code) => $"O código \"{code}\" já está em uso.";

    public static string LayoutTaken(string line, string product, string shift) =>
        $"Já existe um layout para {line}, produto {product}, turno {shift}.";

    public static string PostNameTaken(string name) => $"O nome de posto \"{name}\" está repetido neste layout.";

    public static string FieldTooLong(string field, int max) => $"{field} não pode passar de {max} caracteres.";

    public static string MachineLimit(int max) => $"A pessoa já atingiu o limite de {max} máquinas nesta produção.";

    public static string CriticalPostsEmpty(IReadOnlyList<string> names) =>
        $"Há postos críticos sem ninguém: {string.Join(", ", names)}.";

    public static string LineBusy(string line) => $"A linha {line} já tem uma produção em andamento ou em preparo.";

    public static string PersonBusy(string name, string line) => $"{name} já está alocado(a) na produção da linha {line}.";
}
