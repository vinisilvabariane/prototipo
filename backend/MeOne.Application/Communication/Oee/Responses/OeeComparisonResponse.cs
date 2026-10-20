namespace MeOne.Application.Communication.Oee.Responses;

public sealed record OeeComparisonResponse(
    DateTime From,
    DateTime To,
    IReadOnlyList<OeeRunSummaryResponse> Runs,
    bool LossSourceAvailable,
    string FormulaSetName);
