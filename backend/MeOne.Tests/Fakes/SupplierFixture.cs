using MeOne.Application.Communication.Suppliers.Dtos;
using MeOne.Application.Communication.Suppliers.Requests;
using MeOne.Application.Interfaces;
using MeOne.Application.UseCases.Suppliers;
using MeOne.Domain.Entities;
using MeOne.Domain.Enums;
using MeOne.Domain.Rules;

namespace MeOne.Tests.Fakes;

public sealed class SupplierFixture
{
    public const string ValidCnpj = "11.222.333/0001-81";

    public static readonly byte[] PdfBytes = [.. "%PDF-1.4 sample"u8];

    public static readonly byte[] PngBytes = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3, 4];

    public static readonly byte[] JpegBytes = [0xFF, 0xD8, 0xFF, 0xE0, 0, 16, 1, 2, 3, 4];

    public SupplierFixture()
    {
        Access = new AccessFixture();
        Author = Access.AddUser(3101, "author@example.com", "Quirino Qualidade");
        Other = Access.AddUser(3102, "other@example.com", "Olavo Outro");
        Manager = Access.AddUser(3103, "manager@example.com", "Maria Gestora");
    }

    public AccessFixture Access { get; }

    public FakeSupplierRepository Suppliers { get; } = new();

    public FakeSupplierFileStorage Storage { get; } = new();

    public User Author { get; }

    public User Other { get; }

    public User Manager { get; }

    public FakeCurrentActor AuthorActor => FakeCurrentActor.For(Author);

    public FakeCurrentActor OtherActor => FakeCurrentActor.For(Other);

    public FakeCurrentActor ManagerActor => FakeCurrentActor.WithPermissions(Manager.Id, Permissions.SuppliersRegister, Permissions.SuppliersManage);

    public FakeCurrentActor RegistrarActor => FakeCurrentActor.WithPermissions(Author.Id, Permissions.SuppliersRegister);

    public CreateSupplierUseCase Create(ICurrentActor? actor = null) => new(Suppliers, actor ?? AuthorActor);

    public UpdateSupplierUseCase Update(ICurrentActor? actor = null) => new(Suppliers, actor ?? AuthorActor);

    public GetSupplierUseCase Get(ICurrentActor? actor = null) => new(Suppliers, Access.Users, actor ?? AuthorActor);

    public ListSuppliersUseCase List(ICurrentActor? actor = null) => new(Suppliers, actor ?? AuthorActor);

    public AddSupplierToolUseCase AddTool(ICurrentActor? actor = null) => new(Suppliers, actor ?? AuthorActor);

    public UpdateSupplierToolUseCase UpdateTool(ICurrentActor? actor = null) => new(Suppliers, actor ?? AuthorActor);

    public RemoveSupplierToolUseCase RemoveTool(ICurrentActor? actor = null) => new(Suppliers, actor ?? AuthorActor);

    public AddSupplierMaterialUseCase AddMaterial(ICurrentActor? actor = null) => new(Suppliers, actor ?? AuthorActor);

    public AddSupplierSelfAssessmentUseCase AddSelfAssessment(ICurrentActor? actor = null) => new(Suppliers, Access.Users, actor ?? AuthorActor);

    public UpdateSupplierSelfAssessmentUseCase UpdateSelfAssessment(ICurrentActor? actor = null) => new(Suppliers, Access.Users, actor ?? AuthorActor);

    public AddSupplierAuditUseCase AddAudit(ICurrentActor? actor = null) => new(Suppliers, Access.Users, actor ?? AuthorActor);

    public AddSupplierDocumentUseCase AddDocument(ICurrentActor? actor = null) => new(Suppliers, Storage, actor ?? AuthorActor);

    public RemoveSupplierDocumentUseCase RemoveDocument(ICurrentActor? actor = null) => new(Suppliers, Storage, actor ?? AuthorActor);

    public GetSupplierDocumentUseCase GetDocument() => new(Suppliers, Storage);

    public static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-3));

    public static SaveSupplierRequest Request(
        string sapCode = "10001",
        string name = "Plásticos Alfa",
        MultiBranch branch = MultiBranch.BlueMg,
        SupplierOrigin origin = SupplierOrigin.Domestic,
        string? taxId = ValidCnpj,
        string? email = "contato@alfa.com") =>
        new(sapCode, name, "Matriz", branch, origin, taxId, "Alfa Plásticos Ltda", SupplierCriticality.High, "30110-000", "Centro", "Rua A", "100", "Ana", "31 99999-0000", email);

    public static SupplierFileUpload Pdf(byte[]? bytes = null, string name = "doc.pdf")
    {
        var content = bytes ?? PdfBytes;
        return new SupplierFileUpload(name, content.Length, new MemoryStream(content));
    }

    public static SupplierFileUpload Png() => new("img.png", PngBytes.Length, new MemoryStream(PngBytes));

    public async Task<Supplier> AddAsync(string sapCode = "10001", string name = "Plásticos Alfa", MultiBranch branch = MultiBranch.BlueMg)
    {
        var result = await Create().ExecuteAsync(Request(sapCode, name, branch), CancellationToken.None);
        Assert.True(result.IsSuccess, string.Join(";", result.Errors.Select(error => error.Message)));

        return Suppliers.Stored.Single(supplier => supplier.Id == result.Value.Id);
    }

    public static SaveSelfAssessmentRequest SelfAssessment(ulong responsibleId, DateOnly? on = null, QualificationStatus status = QualificationStatus.Approved, bool? needsAudit = null) =>
        new(responsibleId, on ?? Today, 88.5m, status, "https://example.com/doc", "FORM-1", 2, needsAudit);

    public static SaveAuditRequest Audit(ulong responsibleId, DateOnly? on = null, int months = 12, QualificationStatus status = QualificationStatus.Approved) =>
        new(responsibleId, on ?? Today, months, 92m, status, "CHK-9", 1);
}
