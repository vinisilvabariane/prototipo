using MeOne.Application.Communication.Suppliers.Requests;
using MeOne.Application.Validators.Suppliers;
using MeOne.Domain.Enums;
using MeOne.Tests.Fakes;
using static MeOne.Tests.Fakes.SupplierFixture;

namespace MeOne.Tests.Application;

public sealed class SupplierValidatorTests
{
    private readonly SaveSupplierRequestValidator _supplier = new();

    [Fact]
    public void ValidSupplier_Passes() => Assert.True(_supplier.Validate(Request()).IsValid);

    [Fact]
    public void DomesticSupplier_NeedsAValidCnpjWhenItIsGiven()
    {
        Assert.False(_supplier.Validate(Request(taxId: "11.222.333/0001-82")).IsValid);
        Assert.True(_supplier.Validate(Request(taxId: null)).IsValid);
    }

    [Fact]
    public void ForeignSupplier_AcceptsAFreeTextTaxId() =>
        Assert.True(_supplier.Validate(Request(origin: SupplierOrigin.International, taxId: "EIN 12-3456789")).IsValid);

    [Fact]
    public void InvalidEmail_Fails() => Assert.False(_supplier.Validate(Request(email: "not-an-email")).IsValid);

    [Fact]
    public void RequiredFields_AreEnforced()
    {
        var result = _supplier.Validate(Request() with { SapCode = " ", Name = "", ZipCode = "", District = "" });

        Assert.Equal(4, result.Errors.Select(error => error.PropertyName).Distinct().Count());
    }

    [Fact]
    public void UnknownEnumValues_Fail() =>
        Assert.False(_supplier.Validate(Request() with { Criticality = (SupplierCriticality)9 }).IsValid);

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(100, true)]
    [InlineData(101, false)]
    public void SelfAssessmentScore_StaysBetweenZeroAndOneHundred(int score, bool valid) =>
        Assert.Equal(valid, new SaveSelfAssessmentRequestValidator().Validate(SelfAssessment(1) with { Score = score }).IsValid);

    [Theory]
    [InlineData("https://example.com/x", true)]
    [InlineData("ftp://example.com/x", false)]
    [InlineData("javascript:alert(1)", false)]
    public void SelfAssessmentLink_MustBeHttp(string link, bool valid) =>
        Assert.Equal(valid, new SaveSelfAssessmentRequestValidator().Validate(SelfAssessment(1) with { EditableLink = link }).IsValid);

    [Theory]
    [InlineData(3, true)]
    [InlineData(6, true)]
    [InlineData(12, true)]
    [InlineData(4, false)]
    public void AuditValidity_IsThreeSixOrTwelveMonths(int months, bool valid) =>
        Assert.Equal(valid, new SaveAuditRequestValidator().Validate(Audit(1, months: months)).IsValid);

    [Fact]
    public void Revision_StaysInRange()
    {
        Assert.False(new SaveAuditRequestValidator().Validate(Audit(1) with { ChecklistRevision = 1000 }).IsValid);
        Assert.False(new SaveSelfAssessmentRequestValidator().Validate(SelfAssessment(1) with { Revision = -1 }).IsValid);
    }

    [Fact]
    public void Responsible_IsRequired() =>
        Assert.False(new SaveAuditRequestValidator().Validate(Audit(0)).IsValid);

    [Fact]
    public void Tool_NeedsAName()
    {
        Assert.False(new SaveSupplierToolRequestValidator().Validate(new SaveSupplierToolRequest("", null)).IsValid);
        Assert.True(new SaveSupplierToolRequestValidator().Validate(new SaveSupplierToolRequest("Molde", null)).IsValid);
    }

    [Fact]
    public void Material_NeedsASku()
    {
        Assert.False(new SaveSupplierMaterialRequestValidator().Validate(new SaveSupplierMaterialRequest(" ", null, null, null, null)).IsValid);
        Assert.True(new SaveSupplierMaterialRequestValidator().Validate(new SaveSupplierMaterialRequest("SKU1", null, null, null, null)).IsValid);
    }

    [Fact]
    public void Query_ValidatesPaging()
    {
        var validator = new SuppliersQueryValidator();

        Assert.True(validator.Validate(new SuppliersQuery()).IsValid);
        Assert.False(validator.Validate(new SuppliersQuery { Page = -1 }).IsValid);
        Assert.False(validator.Validate(new SuppliersQuery { PageSize = 0 }).IsValid);
    }
}
