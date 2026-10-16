using MeOne.Application.Communication.WorkInstructions.Dtos;
using MeOne.Application.Communication.WorkInstructions.Requests;
using MeOne.Application.UseCases.WorkInstructions;
using MeOne.Domain.Entities;

namespace MeOne.Tests.Fakes;

public sealed class WorkInstructionFixture
{
    public static readonly byte[] PdfBytes = [.. "%PDF-1.4 sample"u8];

    public static readonly byte[] PngBytes = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3, 4];

    public WorkInstructionFixture()
    {
        Access = new AccessFixture();
        Author = Access.AddUser(3001, "author@example.com", "Elisa Engenheira");
        Reader = Access.AddUser(3002, "reader@example.com", "Rui Operador");
    }

    public AccessFixture Access { get; }

    public FakeWorkInstructionRepository Instructions { get; } = new();

    public FakeWorkInstructionFileStorage Storage { get; } = new();

    public User Author { get; }

    public User Reader { get; }

    public FakeCurrentActor AuthorActor => FakeCurrentActor.For(Author);

    public CreateWorkInstructionUseCase Create => new(Instructions, Storage, AuthorActor);

    public UpdateWorkInstructionUseCase Update => new(Instructions, Storage, AuthorActor);

    public static SaveWorkInstructionRequest Request(string code = "IT-20261015-AAAAA", string title = "Soldar a carcaça", params SaveWorkInstructionStep[] steps) =>
        new(code, title, "DOC-77", "Fone X", "Elisa", "Marcos", "Luvas e óculos", "Temperatura de 350 °C", null, steps);

    public static SaveWorkInstructionStep Step(string text, ulong? id = null, bool removeImage = false) => new(id, text, removeImage);

    public static WorkInstructionUpload Pdf(byte[]? bytes = null)
    {
        var content = bytes ?? PdfBytes;
        return new WorkInstructionUpload("it.pdf", content.Length, new MemoryStream(content));
    }

    public static WorkInstructionUpload Png(byte[]? bytes = null)
    {
        var content = bytes ?? PngBytes;
        return new WorkInstructionUpload("step.png", content.Length, new MemoryStream(content));
    }

    public async Task<WorkInstruction> AddAsync(string code = "IT-20261015-AAAAA", string title = "Soldar a carcaça", int steps = 2, bool withImages = false)
    {
        var drafts = Enumerable.Range(1, steps).Select(index => Step($"Passo {index}")).ToArray();
        var images = Enumerable.Range(0, steps).Select(_ => withImages ? Png() : null).ToList();

        var result = await Create.ExecuteAsync(Request(code, title, drafts), Pdf(), images, CancellationToken.None);
        Assert.True(result.IsSuccess, string.Join(";", result.Errors.Select(error => error.Message)));

        return Instructions.Stored.Single(instruction => instruction.Id == result.Value.Id);
    }
}
