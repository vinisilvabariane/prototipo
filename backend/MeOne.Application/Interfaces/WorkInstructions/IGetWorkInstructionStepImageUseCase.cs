using FluentResults;
using MeOne.Application.Communication.WorkInstructions.Dtos;

namespace MeOne.Application.Interfaces.WorkInstructions;

public interface IGetWorkInstructionStepImageUseCase
{
    Task<Result<WorkInstructionDownload>> ExecuteAsync(ulong id, ulong stepId, CancellationToken cancellationToken);
}
