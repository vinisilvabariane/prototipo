namespace IrisWeb.Application.Communication.Matrix.Dtos;

public sealed record SyncConfiguredProduct(string Product, IReadOnlyList<string>? Shifts);
