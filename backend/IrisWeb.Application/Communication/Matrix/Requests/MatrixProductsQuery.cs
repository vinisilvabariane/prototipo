namespace IrisWeb.Application.Communication.Matrix.Requests;

public sealed record MatrixProductsQuery
{
    public string? Line { get; init; }
}
