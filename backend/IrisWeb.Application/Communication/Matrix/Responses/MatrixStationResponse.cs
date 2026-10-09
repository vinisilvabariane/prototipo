namespace IrisWeb.Application.Communication.Matrix.Responses;

public sealed record MatrixStationResponse(
    int Id,
    string Name,
    bool IsMachine,
    MatrixCollaboratorResponse? Allocated,
    bool IsQuality,
    bool IsCritical,
    bool HasActivity,
    decimal X,
    decimal Y,
    decimal Width,
    decimal Height,
    decimal Rotation);
