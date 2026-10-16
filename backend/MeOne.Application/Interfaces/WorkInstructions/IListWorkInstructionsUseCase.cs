using FluentResults;
using MeOne.Application.Communication.WorkInstructions.Requests;
using MeOne.Application.Communication.WorkInstructions.Responses;

namespace MeOne.Application.Interfaces.WorkInstructions;

public interface IListWorkInstructionsUseCase
{
    Task<Result<WorkInstructionPageResponse>> ExecuteAsync(
        WorkInstructionsQuery query,
        CancellationToken cancellationToken);
}
