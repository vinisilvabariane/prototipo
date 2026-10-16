using FluentResults;
using MeOne.Application.Communication.WorkInstructions.Dtos;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces.WorkInstructions;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.WorkInstructions;

public sealed class GetWorkInstructionStepImageUseCase(IWorkInstructionRepository instructions, IWorkInstructionFileStorage storage)
    : IGetWorkInstructionStepImageUseCase
{
    public async Task<Result<WorkInstructionDownload>> ExecuteAsync(ulong id, ulong stepId, CancellationToken cancellationToken)
    {
        var instruction = await instructions.GetByIdAsync(id, cancellationToken);
        if (instruction is null)
            return Result.Fail<WorkInstructionDownload>(new NotFoundError(WorkInstructionErrors.NotFound));

        var step = instruction.Steps.FirstOrDefault(candidate => candidate.Id == stepId);
        if (step is null)
            return Result.Fail<WorkInstructionDownload>(new NotFoundError(WorkInstructionErrors.StepNotFound));

        if (step.ImageStoredName is null)
            return Result.Fail<WorkInstructionDownload>(new NotFoundError(WorkInstructionErrors.FileNotFound));

        var content = await storage.OpenReadAsync(step.ImageStoredName, cancellationToken);
        if (content is null)
            return Result.Fail<WorkInstructionDownload>(new NotFoundError(WorkInstructionErrors.FileNotFound));

        return Result.Ok(new WorkInstructionDownload(
            content,
            $"{instruction.Code}-passo-{step.Position}",
            step.ImageContentType ?? "application/octet-stream"));
    }
}
