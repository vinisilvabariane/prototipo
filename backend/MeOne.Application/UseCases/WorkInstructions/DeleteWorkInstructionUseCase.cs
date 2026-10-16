using FluentResults;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces.WorkInstructions;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.WorkInstructions;

public sealed class DeleteWorkInstructionUseCase(
    IWorkInstructionRepository instructions,
    IWorkInstructionFileStorage storage) : IDeleteWorkInstructionUseCase
{
    public async Task<Result> ExecuteAsync(ulong id, CancellationToken cancellationToken)
    {
        var instruction = await instructions.GetByIdAsync(id, cancellationToken);
        if (instruction is null)
            return Result.Fail(new NotFoundError(WorkInstructionErrors.NotFound));

        var files = instruction.Steps
            .Where(step => step.ImageStoredName is not null)
            .Select(step => step.ImageStoredName!)
            .Append(instruction.PdfStoredName)
            .ToList();

        instructions.Delete(instruction);

        if (await instructions.SaveChangesAsync(cancellationToken) <= 0)
            return Result.Fail(WorkInstructionErrors.NothingPersisted);

        await WorkInstructionsMapping.DiscardAsync(storage, files);
        return Result.Ok();
    }
}
