using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Suppliers.Responses;

public sealed record SupplierSelfAssessmentResponse(
    ulong Id,
    ulong ResponsibleId,
    string ResponsibleName,
    DateOnly AssessedOn,
    DateOnly ExpiresOn,
    bool Expired,
    decimal? Score,
    QualificationStatus Status,
    string? EditableLink,
    string? DocumentCode,
    int? Revision,
    bool? NeedsOnSiteAudit);
