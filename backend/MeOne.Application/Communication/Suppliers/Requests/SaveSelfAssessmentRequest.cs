using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Suppliers.Requests;

public sealed record SaveSelfAssessmentRequest(
    ulong ResponsibleId,
    DateOnly AssessedOn,
    decimal? Score,
    QualificationStatus Status,
    string? EditableLink,
    string? DocumentCode,
    int? Revision,
    bool? NeedsOnSiteAudit);
