namespace IrisWeb.Application.Communication.Matrix.Dtos;

public sealed record SyncLayoutStation(
    int Id,
    string? Name,
    bool? IsMachine,
    int? CollaboratorId,
    string? CollaboratorName,
    int? CollaboratorRegistration,
    string? CollaboratorLevel,
    int? ActivityId = null,
    bool? IsQuality = null,
    bool? IsCritical = null,
    decimal? X = null,
    decimal? Y = null,
    decimal? Width = null,
    decimal? Height = null,
    decimal? Rotation = null);
