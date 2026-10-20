namespace MeOne.Application.Communication.Oee.Responses;

public sealed record OeeVariableResponse(string Key, string Label, string Description, string Unit, IReadOnlyList<string> Slots);

public sealed record OeeSlotResponse(string Key, string Label, string Group);

public sealed record OeeFunctionResponse(string Name, string Signature, string Description);
