using FluentResults;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Communication.Production.Responses;

namespace MeOne.Application.Interfaces.Production;

public interface IListRunCandidatesUseCase
{
    Task<Result<IReadOnlyList<RunCandidateResponse>>> ExecuteAsync(ulong runId, CandidatesQuery query, CancellationToken cancellationToken);
}
