using FluentResults;
using MeOne.Application.Communication.Production.Requests;

namespace MeOne.Application.Interfaces.Production;

public interface IRecordRunOutputUseCase
{
    Task<Result> ExecuteAsync(ulong runId, RecordOutputRequest request, CancellationToken cancellationToken);
}
