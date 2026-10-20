namespace MeOne.Application.Communication.Oee.Requests;

public sealed record SaveOeeFormulaSetRequest(string? Name, OeeFormulasRequest? Formulas, bool Activate);
