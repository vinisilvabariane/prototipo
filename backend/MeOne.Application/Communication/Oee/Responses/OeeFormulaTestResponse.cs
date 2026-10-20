namespace MeOne.Application.Communication.Oee.Responses;

public sealed record OeeFormulaTestResponse(
    bool Valid,
    string? Error,
    int? Position,
    IReadOnlyList<string> Identifiers,
    double? Result,
    IReadOnlyDictionary<string, double> Values);
