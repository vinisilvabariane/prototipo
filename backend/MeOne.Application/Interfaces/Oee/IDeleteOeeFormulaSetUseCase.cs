using FluentResults;
using MeOne.Application.Communication.Oee.Requests;
using MeOne.Application.Communication.Oee.Responses;

namespace MeOne.Application.Interfaces.Oee;

public interface IDeleteOeeFormulaSetUseCase
{
    Task<Result> ExecuteAsync(ulong id, CancellationToken cancellationToken);
}
