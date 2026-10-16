using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Suppliers.Responses;

public sealed record SupplierDetailResponse(
    ulong Id,
    string SapCode,
    string Name,
    string Branch,
    MultiBranch MultiBranch,
    SupplierOrigin Origin,
    string? TaxId,
    string? CorporateName,
    SupplierCriticality Criticality,
    string ZipCode,
    string District,
    string? Street,
    string? Number,
    string? ContactName,
    string? Phone,
    string? Email,
    string CreatedBy,
    DateTime CreatedAt,
    string UpdatedBy,
    DateTime UpdatedAt,
    SupplierQualificationResponse? Qualification,
    IReadOnlyList<SupplierToolResponse> Tools,
    IReadOnlyList<SupplierMaterialResponse> Materials,
    IReadOnlyList<SupplierSelfAssessmentResponse> SelfAssessments,
    IReadOnlyList<SupplierAuditResponse> Audits,
    IReadOnlyList<SupplierDocumentResponse> Documents,
    bool CanEdit);
