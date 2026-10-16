using MeOne.Domain.Entities;
using MeOne.Domain.Enums;
using MeOne.Domain.Errors;
using MeOne.Domain.Rules;

namespace MeOne.Tests.Domain;

public sealed class SupplierTests
{
    private static readonly DateOnly Day = new(2026, 10, 15);

    private static Supplier NewSupplier(SupplierOrigin origin = SupplierOrigin.Domestic, string? taxId = "11.222.333/0001-81") =>
        Supplier.Create(" ab-1 ", " Alfa ", " Matriz ", MultiBranch.BlueMg, origin, taxId, " Alfa Ltda ", SupplierCriticality.High, "30110-000", "Centro", null, null, null, null, " A@B.COM ", 1, DateTime.UtcNow);

    [Fact]
    public void Create_NormalizesTheFields()
    {
        var supplier = NewSupplier();

        Assert.Equal("AB-1", supplier.SapCode);
        Assert.Equal("Alfa", supplier.Name);
        Assert.Equal("Matriz", supplier.Branch);
        Assert.Equal("11222333000181", supplier.TaxId);
        Assert.Equal("a@b.com", supplier.Email);
        Assert.Null(supplier.Street);
    }

    [Fact]
    public void ForeignSuppliers_KeepTheTaxIdAsTyped()
    {
        var supplier = NewSupplier(SupplierOrigin.International, " EIN 12-3456789 ");

        Assert.Equal("EIN 12-3456789", supplier.TaxId);
    }

    [Fact]
    public void Update_ReportsWhetherAnythingChanged()
    {
        var supplier = NewSupplier();

        var same = supplier.Update("ab-1", "Alfa", "Matriz", MultiBranch.BlueMg, SupplierOrigin.Domestic, "11222333000181", "Alfa Ltda", SupplierCriticality.High, "30110-000", "Centro", null, null, null, null, "a@b.com", 2, DateTime.UtcNow);
        var changed = supplier.Update("ab-1", "Beta", "Matriz", MultiBranch.BlueMg, SupplierOrigin.Domestic, "11222333000181", "Alfa Ltda", SupplierCriticality.High, "30110-000", "Centro", null, null, null, null, "a@b.com", 2, DateTime.UtcNow);

        Assert.False(same);
        Assert.True(changed);
        Assert.Equal(2UL, supplier.UpdatedBy);
        Assert.Equal("Beta", supplier.Name);
    }

    [Theory]
    [InlineData("11.222.333/0001-81", true)]
    [InlineData("11222333000181", true)]
    [InlineData("11.222.333/0001-82", false)]
    [InlineData("00000000000000", false)]
    [InlineData("123", false)]
    public void Cnpj_ValidatesTheCheckDigits(string value, bool valid) => Assert.Equal(valid, Cnpj.IsValid(value));

    [Fact]
    public void SelfAssessment_ExpiresOneYearAndOneDayAfterTheDate()
    {
        var supplier = NewSupplier();
        var assessment = supplier.AddSelfAssessment(5, Day, 80, QualificationStatus.Approved, null, null, null, null);

        Assert.Equal(new DateOnly(2027, 10, 16), assessment.ExpiresOn);
    }

    [Theory]
    [InlineData(3, 2027, 1, 15)]
    [InlineData(6, 2027, 4, 15)]
    [InlineData(12, 2027, 10, 15)]
    public void Audit_ExpiresAfterItsValidityMonths(int months, int year, int month, int day)
    {
        var supplier = NewSupplier();
        var audit = supplier.AddAudit(5, Day, months, 90, QualificationStatus.Approved, null, null);

        Assert.Equal(new DateOnly(year, month, day), audit.ExpiresOn);
    }

    [Fact]
    public void CurrentQualification_IsNullWithoutEvaluations() => Assert.Null(NewSupplier().CurrentQualification);

    [Fact]
    public void CurrentQualification_UsesTheLatestSelfAssessment()
    {
        var supplier = NewSupplier();
        supplier.AddSelfAssessment(5, Day.AddDays(-30), 50, QualificationStatus.Rejected, null, null, null, null);
        supplier.AddSelfAssessment(5, Day, 80, QualificationStatus.Approved, null, null, null, null);

        var current = supplier.CurrentQualification!;

        Assert.Equal(QualificationKind.SelfAssessment, current.Kind);
        Assert.Equal(QualificationStatus.Approved, current.Status);
        Assert.Equal(80, current.Score);
    }

    [Fact]
    public void CurrentQualification_PrefersAnAuditOverASelfAssessment()
    {
        var supplier = NewSupplier();
        supplier.AddSelfAssessment(5, Day, 80, QualificationStatus.Approved, null, null, null, true);
        supplier.AddAudit(5, Day.AddDays(-10), 12, 40, QualificationStatus.Rejected, null, null);

        var current = supplier.CurrentQualification!;

        Assert.Equal(QualificationKind.OnSiteAudit, current.Kind);
        Assert.Equal(QualificationStatus.Rejected, current.Status);
        Assert.False(current.AuditPending);
    }

    [Fact]
    public void CurrentQualification_FlagsAPendingAuditWhenTheSelfAssessmentAsksForOne()
    {
        var supplier = NewSupplier();
        supplier.AddSelfAssessment(5, Day, 80, QualificationStatus.Approved, null, null, null, true);

        Assert.True(supplier.CurrentQualification!.AuditPending);
    }

    [Fact]
    public void RemovedItems_AreHiddenAndIgnoredByTheQualification()
    {
        var supplier = NewSupplier();
        var audit = supplier.AddAudit(5, Day, 12, 40, QualificationStatus.Rejected, null, null);
        supplier.AddSelfAssessment(5, Day, 80, QualificationStatus.Approved, null, null, null, null);

        supplier.RemoveAudit(audit);

        Assert.Empty(supplier.Audits);
        Assert.Equal(QualificationKind.SelfAssessment, supplier.CurrentQualification!.Kind);
    }

    [Theory]
    [InlineData(SupplierStatusFilter.Approved, false, true)]
    [InlineData(SupplierStatusFilter.Expired, true, true)]
    [InlineData(SupplierStatusFilter.Approved, true, false)]
    public void Qualification_MatchesStatusFiltersConsideringExpiry(SupplierStatusFilter filter, bool expired, bool expected)
    {
        var expiresOn = expired ? Day.AddDays(-1) : Day.AddDays(1);
        var qualification = new SupplierQualification(QualificationKind.SelfAssessment, QualificationStatus.Approved, 80, Day.AddYears(-1), expiresOn, false);

        Assert.Equal(expected, qualification.Matches(filter, Day));
    }

    [Fact]
    public void ToolDrawing_NeedsAnExistingTool()
    {
        var supplier = NewSupplier();

        Assert.NotNull(supplier.CheckDocumentTarget(SupplierDocumentKind.ToolDrawing, null));
        Assert.NotNull(supplier.CheckDocumentTarget(SupplierDocumentKind.ToolDrawing, 99));

        supplier.AddTool("Molde", null).Id = 7;

        Assert.Null(supplier.CheckDocumentTarget(SupplierDocumentKind.ToolDrawing, 7));
    }

    [Fact]
    public void SupplierLevelDocuments_RejectAnOwner()
    {
        var supplier = NewSupplier();

        Assert.Null(supplier.CheckDocumentTarget(SupplierDocumentKind.Certificate, null));
        Assert.NotNull(supplier.CheckDocumentTarget(SupplierDocumentKind.Certificate, 1));
    }

    [Fact]
    public void SingleDocuments_ReplaceThePreviousOneOfTheSameOwner()
    {
        var supplier = NewSupplier();

        var first = supplier.AttachDocument(SupplierDocumentKind.Presentation, null, "a.pdf", "s1.pdf", "application/pdf", 10, null, null, 1, DateTime.UtcNow);
        var second = supplier.AttachDocument(SupplierDocumentKind.Presentation, null, "b.pdf", "s2.pdf", "application/pdf", 10, null, null, 1, DateTime.UtcNow);

        Assert.Empty(first);
        Assert.Equal("s1.pdf", Assert.Single(second).StoredName);
        Assert.Equal("s2.pdf", Assert.Single(supplier.Documents).StoredName);
    }

    [Fact]
    public void CollectionDocuments_Accumulate()
    {
        var supplier = NewSupplier();

        supplier.AttachDocument(SupplierDocumentKind.Certificate, null, "a.pdf", "s1.pdf", "application/pdf", 10, "1", Day, 1, DateTime.UtcNow);
        supplier.AttachDocument(SupplierDocumentKind.Certificate, null, "b.pdf", "s2.pdf", "application/pdf", 10, "2", Day, 1, DateTime.UtcNow);

        Assert.Equal(2, supplier.Documents.Count);
    }

    [Fact]
    public void DetachDocument_RemovesAndReturnsIt()
    {
        var supplier = NewSupplier();
        supplier.AttachDocument(SupplierDocumentKind.Certificate, null, "a.pdf", "s1.pdf", "application/pdf", 10, null, null, 1, DateTime.UtcNow);
        supplier.Documents[0].Id = 4;

        Assert.Equal("s1.pdf", supplier.DetachDocument(4)!.StoredName);
        Assert.Empty(supplier.Documents);
        Assert.Null(supplier.DetachDocument(4));
    }

    [Fact]
    public void AttachDocument_ToAMissingOwner_IsADomainViolation() =>
        Assert.Throws<DomainRuleException>(() =>
            NewSupplier().AttachDocument(SupplierDocumentKind.AuditReport, 3, "a.pdf", "s.pdf", "application/pdf", 1, null, null, 1, DateTime.UtcNow));
}
