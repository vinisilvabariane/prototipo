using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Validators.Production;
using MeOne.Domain.Enums;
using static MeOne.Tests.Fakes.ProductionFixture;

namespace MeOne.Tests.Application;

public sealed class ProductionValidatorTests
{
    [Fact]
    public void Line_NeedsAName()
    {
        var validator = new SaveProductionLineRequestValidator();

        Assert.False(validator.Validate(new SaveProductionLineRequest(" ", true)).IsValid);
        Assert.False(validator.Validate(new SaveProductionLineRequest(new string('x', 61), true)).IsValid);
        Assert.True(validator.Validate(new SaveProductionLineRequest("Linha 1", true)).IsValid);
    }

    [Fact]
    public void Shift_NeedsANameAndDifferentTimes()
    {
        var validator = new SaveProductionShiftRequestValidator();

        Assert.False(validator.Validate(new SaveProductionShiftRequest("", new(6, 0), new(14, 0), true)).IsValid);
        Assert.False(validator.Validate(new SaveProductionShiftRequest("T1", new(6, 0), new(6, 0), true)).IsValid);
        Assert.True(validator.Validate(new SaveProductionShiftRequest("Noite", new(22, 0), new(6, 0), true)).IsValid);
    }

    [Fact]
    public void Product_NeedsACodeAndAName()
    {
        var validator = new SaveProductionProductRequestValidator();

        Assert.False(validator.Validate(new SaveProductionProductRequest("", "Fone", true)).IsValid);
        Assert.False(validator.Validate(new SaveProductionProductRequest("P1", "", true)).IsValid);
        Assert.True(validator.Validate(new SaveProductionProductRequest("P1", "Fone", true)).IsValid);
    }

    [Fact]
    public void SaveLayout_NeedsBothListsAndKnownKinds()
    {
        var validator = new SaveLayoutRequestValidator();

        Assert.False(validator.Validate(new SaveLayoutRequest(null!, [])).IsValid);
        Assert.False(validator.Validate(new SaveLayoutRequest([], null!)).IsValid);
        Assert.False(validator.Validate(Layout([Post("A", (PostKind)9)])).IsValid);
        Assert.False(validator.Validate(Layout(decorations: [Decoration((DecorationKind)99)])).IsValid);
        Assert.True(validator.Validate(Layout()).IsValid);
    }

    [Fact]
    public void CopyLayout_NeedsTargetsWithIds()
    {
        var validator = new CopyLayoutRequestValidator();

        Assert.False(validator.Validate(new CopyLayoutRequest(null!)).IsValid);
        Assert.False(validator.Validate(new CopyLayoutRequest([new(0, 1, 1)])).IsValid);
        Assert.True(validator.Validate(new CopyLayoutRequest([new(1, 1, 1)])).IsValid);
    }

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(1_000_000, true)]
    [InlineData(1_000_001, false)]
    public void RecordOutput_KeepsTheQuantityInRange(int quantity, bool valid) =>
        Assert.Equal(valid, new RecordOutputRequestValidator().Validate(new RecordOutputRequest(1, quantity, null)).IsValid);

    [Fact]
    public void RunsQuery_ValidatesPagingAndStatus()
    {
        var validator = new RunsQueryValidator();

        Assert.True(validator.Validate(new RunsQuery()).IsValid);
        Assert.False(validator.Validate(new RunsQuery { Page = -1 }).IsValid);
        Assert.False(validator.Validate(new RunsQuery { PageSize = 0 }).IsValid);
        Assert.False(validator.Validate(new RunsQuery { Status = (RunStatus)7 }).IsValid);
    }

    [Fact]
    public void Allocate_NeedsAPerson() =>
        Assert.False(new AllocatePostRequestValidator().Validate(new AllocatePostRequest(0)).IsValid);
}
