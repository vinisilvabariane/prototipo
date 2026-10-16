using MeOne.Domain.Enums;

namespace MeOne.Application.Communication.Suppliers.Responses;

public sealed record SupplierQualificationResponse(QualificationKind Kind, QualificationStatus Status, decimal? Score, DateOnly PerformedOn, DateOnly ExpiresOn, bool Expired, bool AuditPending);
