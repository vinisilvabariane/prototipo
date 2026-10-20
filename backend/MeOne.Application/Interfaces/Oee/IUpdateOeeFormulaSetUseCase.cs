using FluentResults;
using MeOne.Application.Communication.Oee.Requests;
using MeOne.Application.Communication.Oee.Responses;

namespace MeOne.Application.Interfaces.Oee;

public interface IUpdateOeeFormulaSetUseCase
{
    Task<Result> ExecuteAsync(ulong id, SaveOeeFormulaSetRequest request, CancellationToken cancellationToken);
}
