using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Suppliers.Responses;

public sealed record SupplierAuditResponse(
    ulong Id,
    ulong ResponsibleId,
    string ResponsibleName,
    DateOnly AuditedOn,
    int ValidityMonths,
    DateOnly ExpiresOn,
    bool Expired,
    decimal? Score,
    QualificationStatus Status,
    string? ChecklistCode,
    int? ChecklistRevision);
