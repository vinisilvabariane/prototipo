using MeOne.Application.Communication.WorkInstructions.Requests;
using MeOne.Application.Errors;
using MeOne.Application.UseCases.WorkInstructions;
using MeOne.Domain.Errors;
using MeOne.Tests.Fakes;
using static MeOne.Tests.Fakes.WorkInstructionFixture;

namespace MeOne.Tests.Application;

public sealed class WorkInstructionUseCaseTests
{
    [Fact]
    public async Task Create_StoresTheInstructionStepsAndFiles()
    {
        var f = new WorkInstructionFixture();

        var result = await f.Create.ExecuteAsync(
            Request(steps: [Step("Posicionar a peça"), Step("Soldar")]),
            Pdf(),
            [Png(), null],
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        var stored = Assert.Single(f.Instructions.Stored);
        Assert.Equal("IT-20261015-AAAAA", stored.Code);
        Assert.Equal(f.Author.Id, stored.CreatedBy);
        Assert.Equal([1, 2], stored.Steps.Select(step => step.Position));
        Assert.True(stored.Steps[0].HasImage);
        Assert.False(stored.Steps[1].HasImage);
        Assert.Equal(2, f.Storage.StoredNames.Count);
        Assert.Equal(2, result.Value.StepCount);
    }

    [Fact]
    public async Task Create_NormalizesTheCodeAndRejectsADuplicate()
    {
        var f = new WorkInstructionFixture();
        await f.AddAsync("IT-1");

        var result = await f.Create.ExecuteAsync(Request("it-1"), Pdf(), [], CancellationToken.None);

        Assert.Equal(WorkInstructionErrors.CodeTaken("IT-1"), Assert.Single(result.Errors).Message);
        Assert.Single(f.Instructions.Stored);
    }

    [Fact]
    public async Task Create_WithoutPdf_Fails()
    {
        var f = new WorkInstructionFixture();

        var result = await f.Create.ExecuteAsync(Request(), null, [], CancellationToken.None);

        Assert.Equal(WorkInstructionErrors.PdfRequired, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Create_WithAFileThatIsNotAPdf_Fails()
    {
        var f = new WorkInstructionFixture();

        var result = await f.Create.ExecuteAsync(Request(), Pdf("<html>not a pdf</html>"u8.ToArray()), [], CancellationToken.None);

        Assert.Equal(WorkInstructionErrors.PdfInvalid, Assert.Single(result.Errors).Message);
        Assert.Empty(f.Storage.StoredNames);
    }

    [Fact]
    public async Task Create_WithAnOversizedPdf_Fails()
    {
        var f = new WorkInstructionFixture();
        var huge = new MeOne.Application.Communication.WorkInstructions.Dtos.WorkInstructionUpload("big.pdf", WorkInstructionErrors.MaxPdfBytes + 1, new MemoryStream(PdfBytes));

        var result = await f.Create.ExecuteAsync(Request(), huge, [], CancellationToken.None);

        Assert.Equal(WorkInstructionErrors.PdfTooLarge, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Create_WithAnImageThatIsNotAnImage_FailsAndCleansUp()
    {
        var f = new WorkInstructionFixture();

        var result = await f.Create.ExecuteAsync(
            Request(steps: [Step("Um"), Step("Dois")]),
            Pdf(),
            [Png(), Png("MZ-executable"u8.ToArray())],
            CancellationToken.None);

        Assert.Equal(WorkInstructionErrors.ImageInvalid, Assert.Single(result.Errors).Message);
        Assert.Empty(f.Storage.StoredNames);
        Assert.Empty(f.Instructions.Stored);
    }

    [Fact]
    public async Task Create_WhenNothingIsPersisted_DiscardsTheFiles()
    {
        var f = new WorkInstructionFixture();
        var failing = new FailingSaveRepository();

        var result = await new CreateWorkInstructionUseCase(failing, f.Storage, f.AuthorActor)
            .ExecuteAsync(Request(steps: [Step("Um")]), Pdf(), [Png()], CancellationToken.None);

        Assert.Equal(WorkInstructionErrors.NothingPersisted, Assert.Single(result.Errors).Message);
        Assert.Empty(f.Storage.StoredNames);
    }

    [Fact]
    public async Task Create_WithoutASession_Fails()
    {
        var f = new WorkInstructionFixture();

        var result = await new CreateWorkInstructionUseCase(f.Instructions, f.Storage, FakeCurrentActor.Anonymous())
            .ExecuteAsync(Request(), Pdf(), [], CancellationToken.None);

        Assert.True(result.HasError<CredentialRejectedError>());
    }

    [Fact]
    public async Task Update_ChangesTheFieldsAndTouchesTheAudit()
    {
        var f = new WorkInstructionFixture();
        var instruction = await f.AddAsync();
        var before = instruction.UpdatedAt;
        var editor = FakeCurrentActor.For(f.Reader);

        var result = await new UpdateWorkInstructionUseCase(f.Instructions, f.Storage, editor).ExecuteAsync(
            instruction.Id,
            Request(title: "Soldar a carcaça (rev. B)", steps: [.. instruction.Steps.Select(step => Step(step.Text, step.Id))]),
            null,
            [null, null],
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Soldar a carcaça (rev. B)", instruction.Title);
        Assert.Equal(f.Reader.Id, instruction.UpdatedBy);
        Assert.True(instruction.UpdatedAt >= before);
    }

    [Fact]
    public async Task Update_SyncsStepsKeepingEditingReorderingAddingAndRemoving()
    {
        var f = new WorkInstructionFixture();
        var instruction = await f.AddAsync(steps: 3, withImages: true);
        var one = instruction.Steps[0];
        var two = instruction.Steps[1];
        var three = instruction.Steps[2];

        var result = await f.Update.ExecuteAsync(
            instruction.Id,
            Request(steps: [Step("Passo novo"), Step("Passo 3 editado", three.Id), Step(one.Text, one.Id)]),
            null,
            [null, null, null],
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(["Passo novo", "Passo 3 editado", "Passo 1"], instruction.Steps.Select(step => step.Text));
        Assert.Equal([1, 2, 3], instruction.Steps.Select(step => step.Position));
        Assert.True(instruction.Steps[1].HasImage);
        Assert.False(instruction.Steps[0].HasImage);
        Assert.DoesNotContain(instruction.Steps, step => step.Id == two.Id);
        Assert.Contains(f.Storage.Deleted, name => name.StartsWith("stored-"));
    }

    [Fact]
    public async Task Update_ReplacesAndRemovesStepImages()
    {
        var f = new WorkInstructionFixture();
        var instruction = await f.AddAsync(steps: 2, withImages: true);
        var firstImage = instruction.Steps[0].ImageStoredName;
        var secondImage = instruction.Steps[1].ImageStoredName;

        var result = await f.Update.ExecuteAsync(
            instruction.Id,
            Request(steps: [Step("Passo 1", instruction.Steps[0].Id), Step("Passo 2", instruction.Steps[1].Id, removeImage: true)]),
            null,
            [Png(), null],
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(firstImage, instruction.Steps[0].ImageStoredName);
        Assert.False(instruction.Steps[1].HasImage);
        Assert.Contains(firstImage!, f.Storage.Deleted);
        Assert.Contains(secondImage!, f.Storage.Deleted);
    }

    [Fact]
    public async Task Update_ReplacesThePdfAndDeletesTheOldOne()
    {
        var f = new WorkInstructionFixture();
        var instruction = await f.AddAsync(steps: 0);
        var oldPdf = instruction.PdfStoredName;

        var result = await f.Update.ExecuteAsync(instruction.Id, Request(), Pdf("%PDF-1.7 newer version"u8.ToArray()), [], CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotEqual(oldPdf, instruction.PdfStoredName);
        Assert.Contains(oldPdf, f.Storage.Deleted);
    }

    [Fact]
    public async Task Update_WithAnInvalidPdf_KeepsEverything()
    {
        var f = new WorkInstructionFixture();
        var instruction = await f.AddAsync(steps: 0);
        var oldPdf = instruction.PdfStoredName;

        var result = await f.Update.ExecuteAsync(instruction.Id, Request(title: "Outro"), Pdf("nope"u8.ToArray()), [], CancellationToken.None);

        Assert.Equal(WorkInstructionErrors.PdfInvalid, Assert.Single(result.Errors).Message);
        Assert.Equal(oldPdf, instruction.PdfStoredName);
        Assert.NotEqual("Outro", instruction.Title);
    }

    [Fact]
    public async Task Update_WithACodeUsedByAnotherInstruction_Fails()
    {
        var f = new WorkInstructionFixture();
        await f.AddAsync("IT-A");
        var other = await f.AddAsync("IT-B", "Outra");

        var result = await f.Update.ExecuteAsync(other.Id, Request("it-a"), null, [], CancellationToken.None);

        Assert.Equal(WorkInstructionErrors.CodeTaken("IT-A"), Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Update_KeepingItsOwnCode_Works()
    {
        var f = new WorkInstructionFixture();
        var instruction = await f.AddAsync("IT-A");

        var result = await f.Update.ExecuteAsync(instruction.Id, Request("IT-A"), null, [], CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Update_WithAnUnknownStep_NotFound()
    {
        var f = new WorkInstructionFixture();
        var instruction = await f.AddAsync();

        var result = await f.Update.ExecuteAsync(instruction.Id, Request(steps: [Step("x", 9999)]), null, [null], CancellationToken.None);

        Assert.True(result.HasError<NotFoundError>());
    }

    [Fact]
    public async Task Update_Unknown_NotFound()
    {
        var f = new WorkInstructionFixture();

        var result = await f.Update.ExecuteAsync(404, Request(), null, [], CancellationToken.None);

        Assert.True(result.HasError<NotFoundError>());
    }

    [Fact]
    public async Task Delete_RemovesTheInstructionAndItsFiles()
    {
        var f = new WorkInstructionFixture();
        var instruction = await f.AddAsync(steps: 2, withImages: true);

        var result = await new DeleteWorkInstructionUseCase(f.Instructions, f.Storage).ExecuteAsync(instruction.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(f.Instructions.Stored);
        Assert.Empty(f.Storage.StoredNames);
        Assert.Equal(3, f.Storage.Deleted.Count);
    }

    [Fact]
    public async Task Delete_Unknown_NotFound()
    {
        var f = new WorkInstructionFixture();

        var result = await new DeleteWorkInstructionUseCase(f.Instructions, f.Storage).ExecuteAsync(404, CancellationToken.None);

        Assert.True(result.HasError<NotFoundError>());
    }

    [Fact]
    public async Task List_SearchesTitleCodeProductAndDocumentNumber()
    {
        var f = new WorkInstructionFixture();
        await f.AddAsync("IT-ALFA", "Soldar a carcaça");
        await f.AddAsync("IT-BETA", "Embalar o produto");

        var list = new ListWorkInstructionsUseCase(f.Instructions, f.AuthorActor);

        var byTitle = await list.ExecuteAsync(new WorkInstructionsQuery { Search = "carca" }, CancellationToken.None);
        var byCode = await list.ExecuteAsync(new WorkInstructionsQuery { Search = "beta" }, CancellationToken.None);
        var byDocument = await list.ExecuteAsync(new WorkInstructionsQuery { Search = "doc-77" }, CancellationToken.None);
        var paged = await list.ExecuteAsync(new WorkInstructionsQuery { PageSize = 1, Page = 1 }, CancellationToken.None);

        Assert.Equal(["Soldar a carcaça"], byTitle.Value.Items.Select(item => item.Title));
        Assert.Equal(["IT-BETA"], byCode.Value.Items.Select(item => item.Code));
        Assert.Equal(2, byDocument.Value.TotalCount);
        Assert.Equal(["Soldar a carcaça"], paged.Value.Items.Select(item => item.Title));
    }

    [Fact]
    public async Task Get_ReturnsStepsNamesAndTheManageFlag()
    {
        var f = new WorkInstructionFixture();
        var instruction = await f.AddAsync(steps: 2, withImages: true);

        var asAuthor = await new GetWorkInstructionUseCase(f.Instructions, f.Access.Users, FakeCurrentActor.WithPermissions(f.Author.Id, MeOne.Domain.Rules.Permissions.EngineeringManage))
            .ExecuteAsync(instruction.Id, CancellationToken.None);
        var asReader = await new GetWorkInstructionUseCase(f.Instructions, f.Access.Users, FakeCurrentActor.For(f.Reader))
            .ExecuteAsync(instruction.Id, CancellationToken.None);

        Assert.True(asAuthor.Value.CanManage);
        Assert.False(asReader.Value.CanManage);
        Assert.Equal("Elisa Engenheira", asAuthor.Value.CreatedBy);
        Assert.Equal(["Passo 1", "Passo 2"], asAuthor.Value.Steps.Select(step => step.Text));
        Assert.All(asAuthor.Value.Steps, step => Assert.True(step.HasImage));
    }

    [Fact]
    public async Task Get_Unknown_NotFound()
    {
        var f = new WorkInstructionFixture();

        var result = await new GetWorkInstructionUseCase(f.Instructions, f.Access.Users, f.AuthorActor).ExecuteAsync(404, CancellationToken.None);

        Assert.True(result.HasError<NotFoundError>());
    }

    [Fact]
    public async Task NextCode_ReturnsAFreeCodeInTheOriginalFormat()
    {
        var f = new WorkInstructionFixture();

        var result = await new GetNextWorkInstructionCodeUseCase(f.Instructions).ExecuteAsync(CancellationToken.None);

        Assert.Matches("^IT-\\d{8}-[A-Z2-9]{5}$", result.Value.Code);
    }

    [Fact]
    public async Task Pdf_IsServedNamedAfterTheCode()
    {
        var f = new WorkInstructionFixture();
        var instruction = await f.AddAsync("IT-9");

        var result = await new GetWorkInstructionPdfUseCase(f.Instructions, f.Storage).ExecuteAsync(instruction.Id, CancellationToken.None);

        Assert.Equal("IT-9.pdf", result.Value.FileName);
        Assert.Equal("application/pdf", result.Value.ContentType);
        using var buffer = new MemoryStream();
        await result.Value.Content.CopyToAsync(buffer);
        Assert.Equal(PdfBytes, buffer.ToArray());
    }

    [Fact]
    public async Task Pdf_WhenTheFileIsGone_NotFound()
    {
        var f = new WorkInstructionFixture();
        var instruction = await f.AddAsync();
        await f.Storage.DeleteAsync(instruction.PdfStoredName, CancellationToken.None);

        var result = await new GetWorkInstructionPdfUseCase(f.Instructions, f.Storage).ExecuteAsync(instruction.Id, CancellationToken.None);

        Assert.True(result.HasError<NotFoundError>());
    }

    [Fact]
    public async Task StepImage_IsServedWithItsContentType_AndMissingOnesAreNotFound()
    {
        var f = new WorkInstructionFixture();
        var result = await f.Create.ExecuteAsync(Request(steps: [Step("Com imagem"), Step("Sem imagem")]), Pdf(), [Png(), null], CancellationToken.None);
        var instruction = f.Instructions.Stored.Single(item => item.Id == result.Value.Id);
        var download = new GetWorkInstructionStepImageUseCase(f.Instructions, f.Storage);

        var withImage = await download.ExecuteAsync(instruction.Id, instruction.Steps[0].Id, CancellationToken.None);
        var without = await download.ExecuteAsync(instruction.Id, instruction.Steps[1].Id, CancellationToken.None);
        var unknown = await download.ExecuteAsync(instruction.Id, 9999, CancellationToken.None);

        Assert.Equal("image/png", withImage.Value.ContentType);
        Assert.True(without.HasError<NotFoundError>());
        Assert.True(unknown.HasError<NotFoundError>());
    }

    private sealed class FailingSaveRepository : FakeWorkInstructionRepository
    {
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken) => Task.FromResult(0);
    }
}
