using FluentResults;

namespace MeOne.Application.Interfaces.WorkInstructions;

public interface IDeleteWorkInstructionUseCase
{
    Task<Result> ExecuteAsync(ulong id, CancellationToken cancellationToken);
}
