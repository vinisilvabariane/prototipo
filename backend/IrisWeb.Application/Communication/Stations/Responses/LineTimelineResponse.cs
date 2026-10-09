namespace IrisWeb.Application.Communication.Stations.Responses;

/// <summary>
/// Totais em segundos. Na raiz, WindowSeconds e tempo-posto (janela x postos); no posto, e a
/// janela. Assim OccupiedSeconds / WindowSeconds e a ocupacao nos dois niveis.
/// </summary>
public sealed record LineTimelineTotalsResponse(
    int WindowSeconds,
    int StationCount,
    int OccupiedSeconds,
    int TrainedSeconds,
    int UntrainedSeconds,
    int UnknownLevelSeconds,
    int VacantSeconds,
    int UnknownFaceSeconds,
    int MismatchSeconds);

/// <summary>
/// Um trecho da linha do tempo do posto, ja recortado na janela. Kind: presence, vacancy ou
/// unknown. Training: trained, untrained ou unknown (so em presence). MatchesAllocation e nulo
/// quando nao se sabia quem estava alocado.
/// </summary>
public sealed record LineTimelineSegmentResponse(
    string Kind,
    DateTime StartedAt,
    DateTime EndedAt,
    bool Open,
    string? Registration = null,
    string? Name = null,
    string? TrainingLevel = null,
    string? Training = null,
    string? AllocatedRegistration = null,
    bool? MatchesAllocation = null,
    string? CandidateRegistration = null,
    float? CandidateDistance = null);

public sealed record LineTimelineStationResponse(
    string Workstation,
    int? StationId,
    string? Name,
    LineTimelineTotalsResponse Totals,
    IReadOnlyList<LineTimelineSegmentResponse> Segments);

public sealed record LineTimelinePersonResponse(
    string Registration,
    string? Name,
    string? TrainingLevel,
    string Training,
    int Seconds,
    IReadOnlyList<string> Workstations);

public sealed record LineTimelineResponse(
    string Line,
    DateTime From,
    DateTime To,
    DateTime? FirstActivityAt,
    DateTime? LastActivityAt,
    bool Truncated,
    LineTimelineTotalsResponse Totals,
    IReadOnlyList<LineTimelineStationResponse> Stations,
    IReadOnlyList<LineTimelinePersonResponse> People);
