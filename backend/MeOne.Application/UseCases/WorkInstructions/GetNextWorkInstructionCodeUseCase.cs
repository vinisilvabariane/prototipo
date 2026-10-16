using FluentResults;
using MeOne.Application.Communication.WorkInstructions.Responses;
using MeOne.Application.Interfaces.WorkInstructions;
using MeOne.Domain.Interfaces;
using MeOne.Domain.Rules;

namespace MeOne.Application.UseCases.WorkInstructions;

public sealed class GetNextWorkInstructionCodeUseCase(IWorkInstructionRepository instructions) : IGetNextWorkInstructionCodeUseCase
{
    private const int Attempts = 10;

    public async Task<Result<WorkInstructionCodeResponse>> ExecuteAsync(CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < Attempts; attempt++)
        {
            var code = WorkInstructionCodes.Generate(DateTime.UtcNow, Random.Shared.Next);

            if (!await instructions.CodeExistsAsync(code, null, cancellationToken))
                return Result.Ok(new WorkInstructionCodeResponse(code));
        }

        return Result.Fail("Não foi possível gerar um código livre. Tente novamente.");
    }
}
