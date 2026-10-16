using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Suppliers.Requests;

public sealed record SaveAuditRequest(
    ulong ResponsibleId,
    DateOnly AuditedOn,
    int ValidityMonths,
    decimal? Score,
    QualificationStatus Status,
    string? ChecklistCode,
    int? ChecklistRevision);
