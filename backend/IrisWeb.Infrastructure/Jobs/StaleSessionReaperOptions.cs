namespace IrisWeb.Infrastructure.Jobs;

public sealed class StaleSessionReaperOptions
{
    public const string SectionName = "StaleSessionReaper";

    public int IntervalMinutes { get; init; } = 30;

    /// <summary>
    /// Generoso de proposito: o Sync nao expoe horario real de turno para o iris-web (so nome,
    /// ver CLAUDE.md), entao nao da para fechar no fim do turno certo. 48h e maior que qualquer
    /// turno real, inclusive os que atravessam meia-noite — isto e rede de seguranca para posto
    /// abandonado, nao o mecanismo principal (ver OpenDevice*UseCase.CloseStrayOpenAsync).
    /// </summary>
    public int MaxOpenHours { get; init; } = 48;
}
