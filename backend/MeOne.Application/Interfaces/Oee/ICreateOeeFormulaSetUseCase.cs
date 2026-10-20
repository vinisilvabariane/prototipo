using FluentResults;
using MeOne.Application.Communication.Oee.Requests;
using MeOne.Application.Communication.Oee.Responses;

namespace MeOne.Application.Interfaces.Oee;

public interface ICreateOeeFormulaSetUseCase
{
    Task<Result<OeeFormulaSetResponse>> ExecuteAsync(SaveOeeFormulaSetRequest request, CancellationToken cancellationToken);
}
