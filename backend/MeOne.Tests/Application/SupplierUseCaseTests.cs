using MeOne.Application.Communication.Suppliers.Requests;
using MeOne.Application.Errors;
using MeOne.Domain.Errors;
using MeOne.Domain.Enums;
using MeOne.Tests.Fakes;
using static MeOne.Tests.Fakes.SupplierFixture;

namespace MeOne.Tests.Application;

public sealed class SupplierUseCaseTests
{
    [Fact]
    public async Task Create_StoresTheSupplierForTheActor()
    {
        var f = new SupplierFixture();

        var result = await f.Create().ExecuteAsync(Request(sapCode: " 10001 "), CancellationToken.None);

        Assert.True(result.IsSuccess);
        var stored = Assert.Single(f.Suppliers.Stored);
        Assert.Equal("10001", stored.SapCode);
        Assert.Equal(f.Author.Id, stored.CreatedBy);
        Assert.Null(result.Value.Qualification);
    }

    [Fact]
    public async Task Create_RejectsTheSameSapCodeInTheSameBranch()
    {
        var f = new SupplierFixture();
        await f.AddAsync("10001");

        var result = await f.Create().ExecuteAsync(Request("10001"), CancellationToken.None);

        Assert.Equal(SupplierErrors.SapCodeTaken("10001", "BlueMg"), Assert.Single(result.Errors).Message);
        Assert.Single(f.Suppliers.Stored);
    }

    [Fact]
    public async Task Create_AllowsTheSameSapCodeInAnotherBranch()
    {
        var f = new SupplierFixture();
        await f.AddAsync("10001");

        var result = await f.Create().ExecuteAsync(Request("10001", branch: MultiBranch.GigaAm), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Update_ByTheCreator_ChangesTheSupplier()
    {
        var f = new SupplierFixture();
        var supplier = await f.AddAsync();

        var result = await f.Update().ExecuteAsync(supplier.Id, Request(name: "Novo Nome"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Novo Nome", supplier.Name);
    }

    [Fact]
    public async Task Update_ByAnotherUser_IsForbidden()
    {
        var f = new SupplierFixture();
        var supplier = await f.AddAsync();

        var result = await f.Update(f.OtherActor).ExecuteAsync(supplier.Id, Request(name: "Invasor"), CancellationToken.None);

        Assert.IsType<ForbiddenError>(Assert.Single(result.Errors));
        Assert.Equal("Plásticos Alfa", supplier.Name);
    }

    [Fact]
    public async Task Update_ByAManager_ChangesAnyonesSupplier()
    {
        var f = new SupplierFixture();
        var supplier = await f.AddAsync();

        var result = await f.Update(f.ManagerActor).ExecuteAsync(supplier.Id, Request(name: "Gestor"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(f.Manager.Id, supplier.UpdatedBy);
    }

    [Fact]
    public async Task Update_WithATakenSapCode_Fails()
    {
        var f = new SupplierFixture();
        await f.AddAsync("10001");
        var other = await f.AddAsync("10002", "Beta");

        var result = await f.Update().ExecuteAsync(other.Id, Request("10001", "Beta"), CancellationToken.None);

        Assert.Equal(SupplierErrors.SapCodeTaken("10001", "BlueMg"), Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task Update_WithoutChanges_DoesNotSave()
    {
        var f = new SupplierFixture();
        var supplier = await f.AddAsync();
        var saves = f.Suppliers.SaveCount;

        var result = await f.Update().ExecuteAsync(supplier.Id, Request(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(saves, f.Suppliers.SaveCount);
    }

    [Fact]
    public async Task Update_OfAMissingSupplier_IsNotFound()
    {
        var f = new SupplierFixture();

        var result = await f.Update().ExecuteAsync(99, Request(), CancellationToken.None);

        Assert.IsType<NotFoundError>(Assert.Single(result.Errors));
    }

    [Fact]
    public async Task Tools_CanBeAddedChangedAndRemoved()
    {
        var f = new SupplierFixture();
        var supplier = await f.AddAsync();

        var added = await f.AddTool().ExecuteAsync(supplier.Id, new SaveSupplierToolRequest("Molde 1", "AF-1"), CancellationToken.None);
        Assert.True(added.IsSuccess);

        var changed = await f.UpdateTool().ExecuteAsync(supplier.Id, added.Value.Id, new SaveSupplierToolRequest("Molde 2", null), CancellationToken.None);
        Assert.True(changed.IsSuccess);
        Assert.Equal("Molde 2", Assert.Single(supplier.Tools).Name);

        var removed = await f.RemoveTool().ExecuteAsync(supplier.Id, added.Value.Id, CancellationToken.None);
        Assert.True(removed.IsSuccess);
        Assert.Empty(supplier.Tools);

        var again = await f.RemoveTool().ExecuteAsync(supplier.Id, added.Value.Id, CancellationToken.None);
        Assert.IsType<NotFoundError>(Assert.Single(again.Errors));
    }

    [Fact]
    public async Task Tools_OfAnotherUsersSupplier_AreForbidden()
    {
        var f = new SupplierFixture();
        var supplier = await f.AddAsync();

        var result = await f.AddTool(f.OtherActor).ExecuteAsync(supplier.Id, new SaveSupplierToolRequest("Molde", null), CancellationToken.None);

        Assert.IsType<ForbiddenError>(Assert.Single(result.Errors));
    }

    [Fact]
    public async Task Materials_NormalizeTheSku()
    {
        var f = new SupplierFixture();
        var supplier = await f.AddAsync();

        var result = await f.AddMaterial().ExecuteAsync(supplier.Id, new SaveSupplierMaterialRequest(" abc-1 ", "BU1", "ABS", SupplierCriticality.Low, true), CancellationToken.None);

        Assert.Equal("ABC-1", result.Value.Sku);
        Assert.Equal(SupplierCriticality.Low, result.Value.Criticality);
    }

    [Fact]
    public async Task SelfAssessment_RequiresAnActiveResponsible()
    {
        var f = new SupplierFixture();
        var supplier = await f.AddAsync();

        var result = await f.AddSelfAssessment().ExecuteAsync(supplier.Id, SelfAssessment(9999), CancellationToken.None);

        Assert.IsType<NotFoundError>(Assert.Single(result.Errors));
        Assert.Empty(supplier.SelfAssessments);
    }

    [Fact]
    public async Task SelfAssessment_IsStoredWithItsExpiryAndResponsibleName()
    {
        var f = new SupplierFixture();
        var supplier = await f.AddAsync();

        var result = await f.AddSelfAssessment().ExecuteAsync(supplier.Id, SelfAssessment(f.Other.Id, Today), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(f.Other.Name, result.Value.ResponsibleName);
        Assert.Equal(Today.AddYears(1).AddDays(1), result.Value.ExpiresOn);
        Assert.False(result.Value.Expired);
    }

    [Fact]
    public async Task SelfAssessment_CanBeChanged()
    {
        var f = new SupplierFixture();
        var supplier = await f.AddAsync();
        var added = await f.AddSelfAssessment().ExecuteAsync(supplier.Id, SelfAssessment(f.Other.Id), CancellationToken.None);

        var result = await f.UpdateSelfAssessment().ExecuteAsync(
            supplier.Id, added.Value.Id, SelfAssessment(f.Other.Id, status: QualificationStatus.Rejected), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(QualificationStatus.Rejected, supplier.SelfAssessments.Single().Status);
    }

    [Fact]
    public async Task Detail_ShowsTheAuditAsTheCurrentQualificationAndWhoCanEdit()
    {
        var f = new SupplierFixture();
        var supplier = await f.AddAsync();
        await f.AddSelfAssessment().ExecuteAsync(supplier.Id, SelfAssessment(f.Other.Id), CancellationToken.None);
        await f.AddAudit().ExecuteAsync(supplier.Id, Audit(f.Other.Id, status: QualificationStatus.Rejected), CancellationToken.None);

        var detail = await f.Get().ExecuteAsync(supplier.Id, CancellationToken.None);
        var foreign = await f.Get(f.OtherActor).ExecuteAsync(supplier.Id, CancellationToken.None);

        Assert.Equal(QualificationKind.OnSiteAudit, detail.Value.Qualification!.Kind);
        Assert.Equal(QualificationStatus.Rejected, detail.Value.Qualification.Status);
        Assert.Equal(f.Author.Name, detail.Value.CreatedBy);
        Assert.True(detail.Value.CanEdit);
        Assert.False(foreign.Value.CanEdit);
    }

    [Fact]
    public async Task Audit_RequiresAnActiveResponsible()
    {
        var f = new SupplierFixture();
        var supplier = await f.AddAsync();

        var result = await f.AddAudit().ExecuteAsync(supplier.Id, Audit(9999), CancellationToken.None);

        Assert.IsType<NotFoundError>(Assert.Single(result.Errors));
    }

    [Fact]
    public async Task Detail_OfAMissingSupplier_IsNotFound()
    {
        var f = new SupplierFixture();

        var result = await f.Get().ExecuteAsync(5, CancellationToken.None);

        Assert.IsType<NotFoundError>(Assert.Single(result.Errors));
    }

    [Fact]
    public async Task List_FiltersBySearchAndPages()
    {
        var f = new SupplierFixture();
        await f.AddAsync("1", "Alfa");
        await f.AddAsync("2", "Beta");
        await f.AddAsync("3", "Gama");

        var all = await f.List().ExecuteAsync(new SuppliersQuery { PageSize = 2 }, CancellationToken.None);
        var second = await f.List().ExecuteAsync(new SuppliersQuery { Page = 1, PageSize = 2 }, CancellationToken.None);
        var search = await f.List().ExecuteAsync(new SuppliersQuery { Search = "bet" }, CancellationToken.None);

        Assert.Equal(3, all.Value.TotalCount);
        Assert.Equal(["Alfa", "Beta"], all.Value.Items.Select(item => item.Name));
        Assert.Equal(["Gama"], second.Value.Items.Select(item => item.Name));
        Assert.Equal(["Beta"], search.Value.Items.Select(item => item.Name));
    }

    [Fact]
    public async Task List_FiltersByEvaluationStatus()
    {
        var f = new SupplierFixture();
        var approved = await f.AddAsync("1", "Aprovado");
        var expired = await f.AddAsync("2", "Vencido");
        await f.AddAsync("3", "Sem avaliacao");

        await f.AddSelfAssessment().ExecuteAsync(approved.Id, SelfAssessment(f.Other.Id), CancellationToken.None);
        await f.AddSelfAssessment().ExecuteAsync(expired.Id, SelfAssessment(f.Other.Id, Today.AddYears(-3)), CancellationToken.None);

        async Task<string[]> Names(SupplierStatusFilter status) =>
            [.. (await f.List().ExecuteAsync(new SuppliersQuery { Status = status }, CancellationToken.None)).Value.Items.Select(item => item.Name)];

        Assert.Equal(["Aprovado"], await Names(SupplierStatusFilter.Approved));
        Assert.Equal(["Vencido"], await Names(SupplierStatusFilter.Expired));
        Assert.Equal(["Sem avaliacao"], await Names(SupplierStatusFilter.NotEvaluated));
    }

    [Fact]
    public async Task List_ReportsWhetherTheActorCanRegister()
    {
        var f = new SupplierFixture();

        var registrar = await f.List(f.RegistrarActor).ExecuteAsync(new SuppliersQuery(), CancellationToken.None);
        var reader = await f.List(f.OtherActor).ExecuteAsync(new SuppliersQuery(), CancellationToken.None);

        Assert.True(registrar.Value.CanRegister);
        Assert.False(reader.Value.CanRegister);
    }

    [Fact]
    public async Task AddDocument_StoresThePdfAndRecordsIt()
    {
        var f = new SupplierFixture();
        var supplier = await f.AddAsync();

        var result = await f.AddDocument().ExecuteAsync(
            supplier.Id, new AddSupplierDocumentRequest(SupplierDocumentKind.Certificate, null, "2", Today.AddYears(1)), Pdf(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("application/pdf", result.Value.ContentType);
        Assert.False(result.Value.Expired);
        Assert.Single(f.Storage.StoredNames);
        Assert.Single(supplier.Documents);
    }

    [Fact]
    public async Task AddDocument_AcceptsImagesBySignatureNotByName()
    {
        var f = new SupplierFixture();
        var supplier = await f.AddAsync();

        var result = await f.AddDocument().ExecuteAsync(
            supplier.Id, new AddSupplierDocumentRequest(SupplierDocumentKind.Certificate, null, null, null), Pdf(PngBytes, "fake.pdf"), CancellationToken.None);

        Assert.Equal("image/png", result.Value.ContentType);
    }

    [Fact]
    public async Task AddDocument_RejectsOtherFileTypes()
    {
        var f = new SupplierFixture();
        var supplier = await f.AddAsync();

        var result = await f.AddDocument().ExecuteAsync(
            supplier.Id, new AddSupplierDocumentRequest(SupplierDocumentKind.Certificate, null, null, null), Pdf("<html></html>"u8.ToArray(), "x.pdf"), CancellationToken.None);

        Assert.Equal(SupplierErrors.FileInvalid, Assert.Single(result.Errors).Message);
        Assert.Empty(f.Storage.StoredNames);
    }

    [Fact]
    public async Task AddDocument_RejectsMissingEmptyAndOversizedFiles()
    {
        var f = new SupplierFixture();
        var supplier = await f.AddAsync();
        var request = new AddSupplierDocumentRequest(SupplierDocumentKind.Certificate, null, null, null);

        var missing = await f.AddDocument().ExecuteAsync(supplier.Id, request, null, CancellationToken.None);
        var empty = await f.AddDocument().ExecuteAsync(supplier.Id, request, new("empty.pdf", 0, new MemoryStream()), CancellationToken.None);
        var huge = await f.AddDocument().ExecuteAsync(supplier.Id, request, new("big.pdf", SupplierErrors.MaxFileBytes + 1, new MemoryStream(PdfBytes)), CancellationToken.None);

        Assert.Equal(SupplierErrors.FileRequired, Assert.Single(missing.Errors).Message);
        Assert.Equal(SupplierErrors.FileEmpty, Assert.Single(empty.Errors).Message);
        Assert.Equal(SupplierErrors.FileTooLarge, Assert.Single(huge.Errors).Message);
    }

    [Fact]
    public async Task AddDocument_ForAnItemThatDoesNotExist_IsNotFound()
    {
        var f = new SupplierFixture();
        var supplier = await f.AddAsync();

        var result = await f.AddDocument().ExecuteAsync(
            supplier.Id, new AddSupplierDocumentRequest(SupplierDocumentKind.ToolDrawing, 99, null, null), Pdf(), CancellationToken.None);

        Assert.IsType<NotFoundError>(Assert.Single(result.Errors));
        Assert.Empty(f.Storage.StoredNames);
    }

    [Fact]
    public async Task AddDocument_ForAnItemWithoutTheOwnerId_Fails()
    {
        var f = new SupplierFixture();
        var supplier = await f.AddAsync();

        var result = await f.AddDocument().ExecuteAsync(
            supplier.Id, new AddSupplierDocumentRequest(SupplierDocumentKind.ToolDrawing, null, null, null), Pdf(), CancellationToken.None);

        Assert.Equal(SupplierErrors.DocumentOwnerRequired, Assert.Single(result.Errors).Message);
    }

    [Fact]
    public async Task AddDocument_ReplacingASingleDocument_DeletesTheOldFileAfterTheCommit()
    {
        var f = new SupplierFixture();
        var supplier = await f.AddAsync();
        var request = new AddSupplierDocumentRequest(SupplierDocumentKind.Presentation, null, null, null);

        await f.AddDocument().ExecuteAsync(supplier.Id, request, Pdf(), CancellationToken.None);
        var first = supplier.Documents.Single().StoredName;
        await f.AddDocument().ExecuteAsync(supplier.Id, request, Pdf(), CancellationToken.None);

        Assert.Equal([first], f.Storage.Deleted);
        Assert.Single(supplier.Documents);
        Assert.Single(f.Storage.StoredNames);
    }

    [Fact]
    public async Task AddDocument_ByAnotherUser_IsForbiddenAndStoresNothing()
    {
        var f = new SupplierFixture();
        var supplier = await f.AddAsync();

        var result = await f.AddDocument(f.OtherActor).ExecuteAsync(
            supplier.Id, new AddSupplierDocumentRequest(SupplierDocumentKind.Certificate, null, null, null), Pdf(), CancellationToken.None);

        Assert.IsType<ForbiddenError>(Assert.Single(result.Errors));
        Assert.Empty(f.Storage.StoredNames);
    }

    [Fact]
    public async Task RemoveDocument_DeletesTheFile()
    {
        var f = new SupplierFixture();
        var supplier = await f.AddAsync();
        await f.AddDocument().ExecuteAsync(supplier.Id, new AddSupplierDocumentRequest(SupplierDocumentKind.Certificate, null, null, null), Pdf(), CancellationToken.None);
        var document = supplier.Documents.Single();

        var result = await f.RemoveDocument().ExecuteAsync(supplier.Id, document.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(supplier.Documents);
        Assert.Equal([document.StoredName], f.Storage.Deleted);
    }

    [Fact]
    public async Task RemoveDocument_OfAMissingDocument_IsNotFound()
    {
        var f = new SupplierFixture();
        var supplier = await f.AddAsync();

        var result = await f.RemoveDocument().ExecuteAsync(supplier.Id, 99, CancellationToken.None);

        Assert.IsType<NotFoundError>(Assert.Single(result.Errors));
    }

    [Fact]
    public async Task GetDocument_ReturnsTheStoredBytes()
    {
        var f = new SupplierFixture();
        var supplier = await f.AddAsync();
        await f.AddDocument().ExecuteAsync(supplier.Id, new AddSupplierDocumentRequest(SupplierDocumentKind.Certificate, null, null, null), Pdf(), CancellationToken.None);

        var result = await f.GetDocument().ExecuteAsync(supplier.Id, supplier.Documents.Single().Id, CancellationToken.None);

        using var stream = new MemoryStream();
        await result.Value.Content.CopyToAsync(stream);
        Assert.Equal(PdfBytes, stream.ToArray());
        Assert.Equal("doc.pdf", result.Value.FileName);
    }

    [Fact]
    public async Task GetDocument_WhoseFileIsGone_IsNotFound()
    {
        var f = new SupplierFixture();
        var supplier = await f.AddAsync();
        await f.AddDocument().ExecuteAsync(supplier.Id, new AddSupplierDocumentRequest(SupplierDocumentKind.Certificate, null, null, null), Pdf(), CancellationToken.None);
        await f.Storage.DeleteAsync(supplier.Documents.Single().StoredName, CancellationToken.None);

        var result = await f.GetDocument().ExecuteAsync(supplier.Id, supplier.Documents.Single().Id, CancellationToken.None);

        Assert.IsType<NotFoundError>(Assert.Single(result.Errors));
    }
}
