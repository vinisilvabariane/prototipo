namespace MeOne.Application.Communication.Oee.Requests;

public sealed record TestOeeFormulaRequest(
    string? Slot,
    string? Expression,
    ulong? RunId,
    ulong? PostId,
    IReadOnlyDictionary<string, double>? Values);
