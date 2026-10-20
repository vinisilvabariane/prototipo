using FluentResults;
using MeOne.Application.Communication.Oee.Requests;
using MeOne.Application.Communication.Oee.Responses;

namespace MeOne.Application.Interfaces.Oee;

public interface ITestOeeFormulaUseCase
{
    Task<Result<OeeFormulaTestResponse>> ExecuteAsync(TestOeeFormulaRequest request, CancellationToken cancellationToken);
}
