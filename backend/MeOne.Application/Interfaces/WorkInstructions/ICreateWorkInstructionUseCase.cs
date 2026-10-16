using FluentResults;
using MeOne.Application.Communication.WorkInstructions.Dtos;
using MeOne.Application.Communication.WorkInstructions.Requests;
using MeOne.Application.Communication.WorkInstructions.Responses;

namespace MeOne.Application.Interfaces.WorkInstructions;

public interface ICreateWorkInstructionUseCase
{
    Task<Result<WorkInstructionSummaryResponse>> ExecuteAsync(
        SaveWorkInstructionRequest request,
        WorkInstructionUpload? pdf,
        IReadOnlyList<WorkInstructionUpload?> stepImages,
        CancellationToken cancellationToken);
}
