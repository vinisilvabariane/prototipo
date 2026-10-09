namespace IrisWeb.Application.Communication.Matrix.Requests;

public sealed record AllocateMatrixStationRequest(
    string Line,
    string Product,
    string Shift,
    int StationId,
    string Registration);
