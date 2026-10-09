namespace IrisWeb.Application.Communication.Matrix.Responses;

public sealed record DeviceMatrixResponse(
    string DeviceId,
    string Line,
    string Product,
    string Shift,
    int StationId,
    string StationName,
    DateTime UpdatedAt);
