namespace MeOne.Application.Communication.Oee.Responses;

public sealed record OeeFormulaSetResponse(
    ulong Id,
    string Name,
    bool IsActive,
    OeeFormulasResponse Formulas,
    DateTime UpdatedAt,
    string? UpdatedByName);
