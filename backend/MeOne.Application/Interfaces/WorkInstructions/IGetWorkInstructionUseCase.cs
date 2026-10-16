using FluentResults;
using MeOne.Application.Communication.WorkInstructions.Responses;

namespace MeOne.Application.Interfaces.WorkInstructions;

public interface IGetWorkInstructionUseCase
{
    Task<Result<WorkInstructionDetailResponse>> ExecuteAsync(ulong id, CancellationToken cancellationToken);
}
