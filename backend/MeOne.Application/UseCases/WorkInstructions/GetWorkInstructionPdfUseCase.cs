using FluentResults;
using MeOne.Application.Communication.WorkInstructions.Dtos;
using MeOne.Application.Errors;
using MeOne.Application.Interfaces.WorkInstructions;
using MeOne.Domain.Errors;
using MeOne.Domain.Interfaces;

namespace MeOne.Application.UseCases.WorkInstructions;

public sealed class GetWorkInstructionPdfUseCase(IWorkInstructionRepository instructions, IWorkInstructionFileStorage storage)
    : IGetWorkInstructionPdfUseCase
{
    public async Task<Result<WorkInstructionDownload>> ExecuteAsync(ulong id, CancellationToken cancellationToken)
    {
        var instruction = await instructions.GetByIdAsync(id, cancellationToken);
        if (instruction is null)
            return Result.Fail<WorkInstructionDownload>(new NotFoundError(WorkInstructionErrors.NotFound));

        var content = await storage.OpenReadAsync(instruction.PdfStoredName, cancellationToken);
        if (content is null)
            return Result.Fail<WorkInstructionDownload>(new NotFoundError(WorkInstructionErrors.FileNotFound));

        return Result.Ok(new WorkInstructionDownload(content, $"{instruction.Code}.pdf", "application/pdf"));
    }
}
