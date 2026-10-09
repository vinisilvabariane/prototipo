using IrisWeb.Domain.Enums;

namespace IrisWeb.Domain.Rules;

/// <summary>
/// Horarios dos turnos do Crono Argos, os mesmos do crono-argos antigo. A faixa entre o fim do T2
/// (03:00) e o inicio do ADM (07:28) nao pertence a turno nenhum.
/// </summary>
public static class ArgosShiftRule
{
    public static readonly TimeSpan AdmStart = new(7, 28, 0);

    public static readonly TimeSpan AdmEnd = new(17, 28, 0);

    public static readonly TimeSpan T2Start = new(17, 28, 0);

    public static readonly TimeSpan T2End = new(3, 0, 0);

    public static bool Matches(ArgosShiftFilter shift, DateTime at)
    {
        var time = at.TimeOfDay;

        return shift switch
        {
            ArgosShiftFilter.Adm => time >= AdmStart && time < AdmEnd,
            ArgosShiftFilter.T2 => time >= T2Start || time < T2End,
            _ => true,
        };
    }
}
