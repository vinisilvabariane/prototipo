using FluentResults;
using MeOne.Application.Communication.WorkInstructions.Dtos;
using MeOne.Application.Communication.WorkInstructions.Requests;
using MeOne.Application.Communication.WorkInstructions.Responses;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.WorkInstructions;
using MeOne.Application.UseCases.Auth;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.WorkInstructions;

public sealed class CreateWorkInstructionUseCase(
    IWorkInstructionRepository instructions,
    IWorkInstructionFileStorage storage,
    ICurrentActor currentActor) : ICreateWorkInstructionUseCase
{
    public async Task<Result<WorkInstructionSummaryResponse>> ExecuteAsync(
        SaveWorkInstructionRequest request,
        WorkInstructionUpload? pdf,
        IReadOnlyList<WorkInstructionUpload?> stepImages,
        CancellationToken cancellationToken)
    {
        var actor = await AccessGuard.ActorAsync(currentActor, cancellationToken);
        if (actor.IsFailed)
            return actor.ToResult<WorkInstructionSummaryResponse>();

        var code = WorkInstruction.NormalizeCode(request.Code);
        if (await instructions.CodeExistsAsync(code, null, cancellationToken))
            return Result.Fail<WorkInstructionSummaryResponse>(WorkInstructionErrors.CodeTaken(code));

        if (pdf is null)
            return Result.Fail<WorkInstructionSummaryResponse>(WorkInstructionErrors.PdfRequired);

        if (await WorkInstructionsMapping.PdfProblemAsync(pdf, cancellationToken) is { } pdfProblem)
            return Result.Fail<WorkInstructionSummaryResponse>(pdfProblem);

        var steps = request.Steps ?? [];
        var stored = new List<string>();

        try
        {
            var built = new List<WorkInstructionStep>();

            for (var index = 0; index < steps.Length; index++)
            {
                var image = index < stepImages.Count ? stepImages[index] : null;
                string? imageName = null;
                string? imageType = null;

                if (image is not null)
                {
                    var (contentType, problem) = await WorkInstructionsMapping.ImageAsync(image, cancellationToken);
                    if (problem is not null)
                    {
                        await WorkInstructionsMapping.DiscardAsync(storage, stored);
                        return Result.Fail<WorkInstructionSummaryResponse>(problem);
                    }

                    imageType = contentType!;
                    imageName = await storage.SaveAsync(image.Content, WorkInstructionsMapping.ExtensionFor(imageType), cancellationToken);
                    stored.Add(imageName);
                }

                built.Add(WorkInstructionStep.Create(steps[index].Text, imageName, imageType));
            }

            var pdfName = await storage.SaveAsync(pdf.Content, "pdf", cancellationToken);
            stored.Add(pdfName);

            var now = DateTime.UtcNow;
            var instruction = WorkInstruction.Create(
                request.Code,
                request.Title,
                request.DocumentNumber,
                request.Product,
                request.PreparedBy,
                request.ApprovedBy,
                request.RequiredPpe,
                request.ProcessSpecifications,
                request.Notes,
                pdfName,
                pdf.Length,
                actor.Value.UserId,
                now);

            instruction.SetSteps(built, actor.Value.UserId, now);

            await instructions.AddAsync(instruction, cancellationToken);

            if (await instructions.SaveChangesAsync(cancellationToken) <= 0)
            {
                await WorkInstructionsMapping.DiscardAsync(storage, stored);
                return Result.Fail<WorkInstructionSummaryResponse>(WorkInstructionErrors.NothingPersisted);
            }

            return Result.Ok(WorkInstructionsMapping.ToSummary(instruction));
        }
        catch
        {
            await WorkInstructionsMapping.DiscardAsync(storage, stored);
            throw;
        }
    }
}
