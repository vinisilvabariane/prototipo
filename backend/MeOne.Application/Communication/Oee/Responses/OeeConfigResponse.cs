namespace MeOne.Application.Communication.Oee.Responses;

public sealed record OeeConfigResponse(
    IReadOnlyList<OeeFormulaSetResponse> Sets,
    ulong? ActiveSetId,
    OeeFormulasResponse Default,
    IReadOnlyList<OeeVariableResponse> Variables,
    IReadOnlyList<OeeSlotResponse> Slots,
    IReadOnlyList<OeeFunctionResponse> Functions,
    bool LossSourceAvailable,
    bool CanManage);
