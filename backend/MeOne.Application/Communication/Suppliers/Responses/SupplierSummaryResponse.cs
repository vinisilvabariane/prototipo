using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Suppliers.Responses;

public sealed record SupplierSummaryResponse(
    ulong Id,
    string SapCode,
    string Name,
    string Branch,
    MultiBranch MultiBranch,
    SupplierOrigin Origin,
    SupplierCriticality Criticality,
    SupplierQualificationResponse? Qualification);
