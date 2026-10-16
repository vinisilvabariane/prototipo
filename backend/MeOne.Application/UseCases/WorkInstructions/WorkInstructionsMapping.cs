using MeOne.Application.Communication.WorkInstructions.Dtos;
using MeOne.Application.Communication.WorkInstructions.Responses;
using MeOne.Application.Interfaces.WorkInstructions;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.WorkInstructions;

internal static class WorkInstructionsMapping
{
    public static WorkInstructionSummaryResponse ToSummary(WorkInstruction instruction) => new(
        instruction.Id,
        instruction.Code,
        instruction.Title,
        instruction.DocumentNumber,
        instruction.Product,
        instruction.PreparedBy,
        instruction.ApprovedBy,
        instruction.Steps.Count,
        instruction.PdfSize,
        instruction.CreatedAt,
        instruction.UpdatedAt);

    public static WorkInstructionDetailResponse ToDetail(
        WorkInstruction instruction,
        string createdBy,
        string updatedBy,
        bool canManage) => new(
        instruction.Id,
        instruction.Code,
        instruction.Title,
        instruction.DocumentNumber,
        instruction.Product,
        instruction.PreparedBy,
        instruction.ApprovedBy,
        instruction.RequiredPpe,
        instruction.ProcessSpecifications,
        instruction.Notes,
        instruction.PdfSize,
        createdBy,
        instruction.CreatedAt,
        updatedBy,
        instruction.UpdatedAt,
        [.. instruction.Steps.OrderBy(step => step.Position)
            .Select(step => new WorkInstructionStepResponse(step.Id, step.Position, step.Text, step.HasImage))],
        canManage);

    public static async Task<byte[]> HeaderAsync(WorkInstructionUpload upload, CancellationToken cancellationToken)
    {
        if (!upload.Content.CanSeek)
            return [];

        var buffer = new byte[FileSignatures.HeaderLength];
        upload.Content.Position = 0;
        var read = await upload.Content.ReadAsync(buffer, cancellationToken);
        upload.Content.Position = 0;

        return buffer[..read];
    }

    public static async Task<string?> PdfProblemAsync(WorkInstructionUpload upload, CancellationToken cancellationToken)
    {
        if (upload.Length <= 0)
            return WorkInstructionErrors.FileEmpty;

        if (upload.Length > WorkInstructionErrors.MaxPdfBytes)
            return WorkInstructionErrors.PdfTooLarge;

        return FileSignatures.IsPdf(await HeaderAsync(upload, cancellationToken)) ? null : WorkInstructionErrors.PdfInvalid;
    }

    public static async Task<(string? ContentType, string? Problem)> ImageAsync(
        WorkInstructionUpload upload,
        CancellationToken cancellationToken)
    {
        if (upload.Length <= 0)
            return (null, WorkInstructionErrors.FileEmpty);

        if (upload.Length > WorkInstructionErrors.MaxImageBytes)
            return (null, WorkInstructionErrors.ImageTooLarge);

        var contentType = FileSignatures.ImageContentTypeOf(await HeaderAsync(upload, cancellationToken));

        return contentType is null ? (null, WorkInstructionErrors.ImageInvalid) : (contentType, null);
    }

    public static string ExtensionFor(string contentType) => contentType switch
    {
        "image/png" => "png",
        "image/jpeg" => "jpg",
        "image/gif" => "gif",
        "image/webp" => "webp",
        _ => string.Empty,
    };

    public static async Task DiscardAsync(
        IWorkInstructionFileStorage storage,
        IEnumerable<string> storedNames)
    {
        foreach (var storedName in storedNames)
            await storage.DeleteAsync(storedName, CancellationToken.None);
    }
}
