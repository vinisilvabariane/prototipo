using FluentResults;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Communication.Production.Responses;

namespace MeOne.Application.Interfaces.Production;

public interface ICreateRunUseCase
{
    Task<Result<RunSummaryResponse>> ExecuteAsync(CreateRunRequest request, CancellationToken cancellationToken);
}
