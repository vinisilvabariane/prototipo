namespace IrisWeb.Application.Communication.Matrix.Requests;

public sealed record BindDeviceMatrixRequest(string Line, string Product, string Shift, int StationId);
