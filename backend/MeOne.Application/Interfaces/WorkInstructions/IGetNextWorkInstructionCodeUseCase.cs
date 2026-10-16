using FluentResults;
using MeOne.Application.Communication.WorkInstructions.Responses;

namespace MeOne.Application.Interfaces.WorkInstructions;

public interface IGetNextWorkInstructionCodeUseCase
{
    Task<Result<WorkInstructionCodeResponse>> ExecuteAsync(CancellationToken cancellationToken);
}
