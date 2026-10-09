namespace IrisWeb.Application.Communication.Matrix.Responses;

public sealed record MatrixProductResponse(string Product, IReadOnlyList<string> Shifts);
