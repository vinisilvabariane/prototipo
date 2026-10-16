using FluentResults;
using MeOne.Application.Communication.WorkInstructions.Dtos;
using MeOne.Application.Communication.WorkInstructions.Requests;

namespace MeOne.Application.Interfaces.WorkInstructions;

public interface IUpdateWorkInstructionUseCase
{
    Task<Result> ExecuteAsync(
        ulong id,
        SaveWorkInstructionRequest request,
        WorkInstructionUpload? pdf,
        IReadOnlyList<WorkInstructionUpload?> stepImages,
        CancellationToken cancellationToken);
}
