using MeOne.Domain.Enums;
using MeOne.Domain.Rules;

namespace MeOne.Domain.Entities;

public sealed class SupplierAudit : Entity<ulong>
{
    private SupplierAudit() { }

    public ulong SupplierId { get; private set; }

    public ulong ResponsibleId { get; private set; }

    public DateOnly AuditedOn { get; private set; }

    public int ValidityMonths { get; private set; }

    public DateOnly ExpiresOn { get; private set; }

    public decimal? Score { get; private set; }

    public QualificationStatus Status { get; private set; }

    public string? ChecklistCode { get; private set; }

    public int? ChecklistRevision { get; private set; }

    public bool Active { get; private set; } = true;

    internal static SupplierAudit Create(
        ulong responsibleId,
        DateOnly auditedOn,
        int validityMonths,
        decimal? score,
        QualificationStatus status,
        string? checklistCode,
        int? checklistRevision)
    {
        var audit = new SupplierAudit();
        audit.Update(responsibleId, auditedOn, validityMonths, score, status, checklistCode, checklistRevision);
        return audit;
    }

    public bool Update(
        ulong responsibleId,
        DateOnly auditedOn,
        int validityMonths,
        decimal? score,
        QualificationStatus status,
        string? checklistCode,
        int? checklistRevision)
    {
        var next = (
            ResponsibleId: responsibleId,
            AuditedOn: auditedOn,
            ValidityMonths: validityMonths,
            ExpiresOn: QualificationRules.AuditExpiry(auditedOn, validityMonths),
            Score: score,
            Status: status,
            ChecklistCode: string.IsNullOrWhiteSpace(checklistCode) ? null : checklistCode.Trim(),
            ChecklistRevision: checklistRevision);

        var current = (ResponsibleId, AuditedOn, ValidityMonths, ExpiresOn, Score, Status, ChecklistCode, ChecklistRevision);

        if (current == next)
            return false;

        (ResponsibleId, AuditedOn, ValidityMonths, ExpiresOn, Score, Status, ChecklistCode, ChecklistRevision) = next;
        return true;
    }

    internal void Remove() => Active = false;
}
