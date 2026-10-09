namespace IrisWeb.Application.Communication.Stations.Requests;

/// <summary>
/// Linha + dia + janela opcional (o turno). Sem janela vale o dia inteiro; "ate" menor ou
/// igual ao "de" atravessa a meia-noite (turno da noite).
/// </summary>
public sealed record LineTimelineQuery
{
    public string? Line { get; init; }

    public DateOnly? Date { get; init; }

    public TimeOnly? From { get; init; }

    public TimeOnly? To { get; init; }
}
