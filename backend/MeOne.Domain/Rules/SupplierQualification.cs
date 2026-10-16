using MeOne.Domain.Enums;

namespace MeOne.Domain.Rules;

public sealed record SupplierQualification(
    QualificationKind Kind,
    QualificationStatus Status,
    decimal? Score,
    DateOnly PerformedOn,
    DateOnly ExpiresOn,
    bool AuditPending)
{
    public bool IsExpired(DateOnly today) => ExpiresOn < today;

    public bool Matches(SupplierStatusFilter filter, DateOnly today) => filter switch
    {
        SupplierStatusFilter.Expired => IsExpired(today),
        SupplierStatusFilter.Approved => !IsExpired(today) && Status == QualificationStatus.Approved,
        SupplierStatusFilter.Rejected => !IsExpired(today) && Status == QualificationStatus.Rejected,
        SupplierStatusFilter.Discontinued => !IsExpired(today) && Status == QualificationStatus.Discontinued,
        _ => false,
    };
}
