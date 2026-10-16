using FluentResults;
using MeOne.Application.Communication.WorkInstructions.Dtos;

namespace MeOne.Application.Interfaces.WorkInstructions;

public interface IGetWorkInstructionPdfUseCase
{
    Task<Result<WorkInstructionDownload>> ExecuteAsync(ulong id, CancellationToken cancellationToken);
}
