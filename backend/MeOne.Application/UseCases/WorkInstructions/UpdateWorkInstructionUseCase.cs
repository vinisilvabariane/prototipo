using FluentResults;
using MeOne.Application.Communication.WorkInstructions.Dtos;
using MeOne.Application.Communication.WorkInstructions.Requests;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces;
using MeOne.Application.Interfaces.WorkInstructions;
using MeOne.Application.UseCases.Auth;
using MeOne.Domain.Entities;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.WorkInstructions;

public sealed class UpdateWorkInstructionUseCase(
    IWorkInstructionRepository instructions,
    IWorkInstructionFileStorage storage,
    ICurrentActor currentActor) : IUpdateWorkInstructionUseCase
{
    public async Task<Result> ExecuteAsync(
        ulong id,
        SaveWorkInstructionRequest request,
        WorkInstructionUpload? pdf,
        IReadOnlyList<WorkInstructionUpload?> stepImages,
        CancellationToken cancellationToken)
    {
        var actor = await AccessGuard.ActorAsync(currentActor, cancellationToken);
        if (actor.IsFailed)
            return actor.ToResult();

        var instruction = await instructions.GetByIdAsync(id, cancellationToken);
        if (instruction is null)
            return Result.Fail(new NotFoundError(WorkInstructionErrors.NotFound));

        var code = WorkInstruction.NormalizeCode(request.Code);
        if (await instructions.CodeExistsAsync(code, id, cancellationToken))
            return Result.Fail(WorkInstructionErrors.CodeTaken(code));

        if (pdf is not null && await WorkInstructionsMapping.PdfProblemAsync(pdf, cancellationToken) is { } pdfProblem)
            return Result.Fail(pdfProblem);

        var drafts = request.Steps ?? [];
        var stored = new List<string>();
        var obsolete = new List<string>();
        var touched = false;

        try
        {
            var ordered = new List<WorkInstructionStep>();

            for (var index = 0; index < drafts.Length; index++)
            {
                var draft = drafts[index];
                var image = index < stepImages.Count ? stepImages[index] : null;

                WorkInstructionStep step;

                if (draft.Id is { } stepId)
                {
                    var existing = instruction.Steps.FirstOrDefault(candidate => candidate.Id == stepId);
                    if (existing is null)
                    {
                        await WorkInstructionsMapping.DiscardAsync(storage, stored);
                        return Result.Fail(new NotFoundError(WorkInstructionErrors.StepNotFound));
                    }

                    touched |= existing.Edit(draft.Text);
                    step = existing;
                }
                else
                {
                    step = WorkInstructionStep.Create(draft.Text, null, null);
                }

                if (image is not null)
                {
                    var (contentType, problem) = await WorkInstructionsMapping.ImageAsync(image, cancellationToken);
                    if (problem is not null)
                    {
                        await WorkInstructionsMapping.DiscardAsync(storage, stored);
                        return Result.Fail(problem);
                    }

                    var imageName = await storage.SaveAsync(image.Content, WorkInstructionsMapping.ExtensionFor(contentType!), cancellationToken);
                    stored.Add(imageName);

                    touched = true;

                    if (step.SetImage(imageName, contentType!) is { } replaced)
                        obsolete.Add(replaced);
                }
                else if (draft.RemoveImage && step.ClearImage() is { } removedImage)
                {
                    touched = true;
                    obsolete.Add(removedImage);
                }

                ordered.Add(step);
            }

            var now = DateTime.UtcNow;

            instruction.Update(
                request.Code,
                request.Title,
                request.DocumentNumber,
                request.Product,
                request.PreparedBy,
                request.ApprovedBy,
                request.RequiredPpe,
                request.ProcessSpecifications,
                request.Notes,
                actor.Value.UserId,
                now);

            if (pdf is not null)
            {
                var pdfName = await storage.SaveAsync(pdf.Content, "pdf", cancellationToken);
                stored.Add(pdfName);
                obsolete.Add(instruction.ReplacePdf(pdfName, pdf.Length, actor.Value.UserId, now));
            }

            var removed = instruction.SetSteps(ordered, actor.Value.UserId, now);

            if (touched)
                instruction.MarkEdited(actor.Value.UserId, now);

            obsolete.AddRange(removed.Where(step => step.ImageStoredName is not null).Select(step => step.ImageStoredName!));

            instructions.Update(instruction);

            if (await instructions.SaveChangesAsync(cancellationToken) <= 0)
            {
                await WorkInstructionsMapping.DiscardAsync(storage, stored);
                return Result.Fail(WorkInstructionErrors.NothingPersisted);
            }

            await WorkInstructionsMapping.DiscardAsync(storage, obsolete);
            return Result.Ok();
        }
        catch
        {
            await WorkInstructionsMapping.DiscardAsync(storage, stored);
            throw;
        }
    }
}
