namespace IrisWeb.Application.Interfaces.Stations;

/// <summary>Uma linha da planilha: presença, posto vazio ou rosto desconhecido, no mesmo formato.</summary>
public sealed record StationExportRow(
    string Kind,
    string Line,
    string Workstation,
    string? Registration,
    string? CollaboratorName,
    string? TrainingLevel,
    string? AllocatedRegistration,
    DateTime StartedAt,
    DateTime? EndedAt,
    int DurationSeconds,
    string? CandidateRegistration,
    float? CandidateDistance);

public interface IStationExportWriter
{
    /// <summary>Monta o .xlsx e devolve o stream posicionado no início.</summary>
    Stream Write(IReadOnlyList<StationExportRow> rows, DateTime from, DateTime to, bool truncated);
}
