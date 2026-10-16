using MeOne.Domain.Entities;
using MeOne.Domain.Rules;

namespace MeOne.Tests.Domain;

public sealed class WorkInstructionTests
{
    private static readonly DateTime Now = new(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc);

    private static WorkInstruction NewInstruction() => WorkInstruction.Create(
        " it-1 ", " Soldar ", " DOC ", " Produto ", " Ana ", " Beto ", " Luvas ", " Specs ", "  ", "stored.pdf", 10, 1, Now);

    [Fact]
    public void Create_NormalizesTextAndCode()
    {
        var instruction = NewInstruction();

        Assert.Equal("IT-1", instruction.Code);
        Assert.Equal("Soldar", instruction.Title);
        Assert.Null(instruction.Notes);
        Assert.Equal(Now, instruction.CreatedAt);
        Assert.Equal(instruction.CreatedBy, instruction.UpdatedBy);
    }

    [Fact]
    public void Update_ReportsChangesAndTouchesTheAuditFields()
    {
        var instruction = NewInstruction();
        var later = Now.AddHours(1);

        Assert.False(instruction.Update("IT-1", "Soldar", "DOC", "Produto", "Ana", "Beto", "Luvas", "Specs", null, 2, later));
        Assert.Equal(Now, instruction.UpdatedAt);

        Assert.True(instruction.Update("IT-1", "Soldar a carcaça", "DOC", "Produto", "Ana", "Beto", "Luvas", "Specs", " Cuidado ", 2, later));
        Assert.Equal("Cuidado", instruction.Notes);
        Assert.Equal(later, instruction.UpdatedAt);
        Assert.Equal(2UL, instruction.UpdatedBy);
    }

    [Fact]
    public void ReplacePdf_ReturnsThePreviousFile()
    {
        var instruction = NewInstruction();

        var previous = instruction.ReplacePdf("new.pdf", 20, 2, Now.AddMinutes(5));

        Assert.Equal("stored.pdf", previous);
        Assert.Equal("new.pdf", instruction.PdfStoredName);
        Assert.Equal(20, instruction.PdfSize);
    }

    [Fact]
    public void SetSteps_NumbersThemInOrderAndReportsRemovedOnes()
    {
        var instruction = NewInstruction();
        var first = WorkInstructionStep.Create("Um", null, null);
        var second = WorkInstructionStep.Create("Dois", "img.png", "image/png");
        var third = WorkInstructionStep.Create("Três", null, null);

        instruction.SetSteps([first, second, third], 1, Now);
        Assert.Equal([1, 2, 3], instruction.Steps.Select(step => step.Position));

        var removed = instruction.SetSteps([third, first], 1, Now);

        Assert.Equal([second], removed);
        Assert.Equal(["Três", "Um"], instruction.Steps.Select(step => step.Text));
        Assert.Equal([1, 2], instruction.Steps.Select(step => step.Position));
    }

    [Fact]
    public void SetSteps_WithTheSameOrder_DoesNotTouchTheInstruction()
    {
        var instruction = NewInstruction();
        var step = WorkInstructionStep.Create("Um", null, null);
        instruction.SetSteps([step], 1, Now);

        instruction.SetSteps([step], 2, Now.AddHours(2));

        Assert.Equal(Now, instruction.UpdatedAt);
    }

    [Fact]
    public void Step_ImageAndTextEditsReportChanges()
    {
        var step = WorkInstructionStep.Create(" Um ", null, "image/png");

        Assert.Null(step.ImageContentType);
        Assert.False(step.HasImage);
        Assert.False(step.Edit("Um"));
        Assert.True(step.Edit("Dois"));

        Assert.Null(step.SetImage("a.png", "image/png"));
        Assert.True(step.HasImage);
        Assert.Equal("a.png", step.SetImage("b.png", "image/png"));
        Assert.Equal("b.png", step.ClearImage());
        Assert.Null(step.ClearImage());
    }

    [Fact]
    public void FileSignatures_RecognizeTheAllowedTypes()
    {
        Assert.True(FileSignatures.IsPdf("%PDF-1.7"u8));
        Assert.False(FileSignatures.IsPdf("<html>"u8));
        Assert.False(FileSignatures.IsPdf([]));

        Assert.Equal("image/png", FileSignatures.ImageContentTypeOf([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]));
        Assert.Equal("image/jpeg", FileSignatures.ImageContentTypeOf([0xFF, 0xD8, 0xFF, 0xE0]));
        Assert.Equal("image/gif", FileSignatures.ImageContentTypeOf("GIF89a"u8));
        Assert.Equal("image/webp", FileSignatures.ImageContentTypeOf("RIFF1234WEBPVP8 "u8));
        Assert.Null(FileSignatures.ImageContentTypeOf("MZ-not-an-image"u8));
    }

    [Fact]
    public void Codes_FollowTheOriginalFormat()
    {
        var code = WorkInstructionCodes.Generate(new DateTime(2026, 10, 15, 0, 0, 0, DateTimeKind.Utc), _ => 0);

        Assert.Equal("IT-20261015-AAAAA", code);
        Assert.Matches("^IT-\\d{8}-[A-Z2-9]{5}$", WorkInstructionCodes.Generate(DateTime.UtcNow, Random.Shared.Next));
    }
}
