namespace IrisWeb.Application.Communication.Matrix.Requests;

public sealed record MatrixStationsQuery
{
    public string? Line { get; init; }

    public string? Product { get; init; }

    public string? Shift { get; init; }
}
