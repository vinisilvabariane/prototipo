using MeOne.Domain.Enums;
using MeOne.Domain.Rules;

namespace MeOne.Domain.Entities;

public sealed class SupplierSelfAssessment : Entity<ulong>
{
    private SupplierSelfAssessment() { }

    public ulong SupplierId { get; private set; }

    public ulong ResponsibleId { get; private set; }

    public DateOnly AssessedOn { get; private set; }

    public DateOnly ExpiresOn { get; private set; }

    public decimal? Score { get; private set; }

    public QualificationStatus Status { get; private set; }

    public string? EditableLink { get; private set; }

    public string? DocumentCode { get; private set; }

    public int? Revision { get; private set; }

    public bool? NeedsOnSiteAudit { get; private set; }

    public bool Active { get; private set; } = true;

    internal static SupplierSelfAssessment Create(
        ulong responsibleId,
        DateOnly assessedOn,
        decimal? score,
        QualificationStatus status,
        string? editableLink,
        string? documentCode,
        int? revision,
        bool? needsOnSiteAudit)
    {
        var assessment = new SupplierSelfAssessment();
        assessment.Update(responsibleId, assessedOn, score, status, editableLink, documentCode, revision, needsOnSiteAudit);
        return assessment;
    }

    public bool Update(
        ulong responsibleId,
        DateOnly assessedOn,
        decimal? score,
        QualificationStatus status,
        string? editableLink,
        string? documentCode,
        int? revision,
        bool? needsOnSiteAudit)
    {
        var next = (
            ResponsibleId: responsibleId,
            AssessedOn: assessedOn,
            ExpiresOn: QualificationRules.SelfAssessmentExpiry(assessedOn),
            Score: score,
            Status: status,
            EditableLink: Clean(editableLink),
            DocumentCode: Clean(documentCode),
            Revision: revision,
            NeedsOnSiteAudit: needsOnSiteAudit);

        var current = (ResponsibleId, AssessedOn, ExpiresOn, Score, Status, EditableLink, DocumentCode, Revision, NeedsOnSiteAudit);

        if (current == next)
            return false;

        (ResponsibleId, AssessedOn, ExpiresOn, Score, Status, EditableLink, DocumentCode, Revision, NeedsOnSiteAudit) = next;
        return true;
    }

    internal void Remove() => Active = false;

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
