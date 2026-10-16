using FluentResults;
using MeOne.Application.Communication.Production.Requests;

namespace MeOne.Application.Interfaces.Production;

public interface IAllocatePostUseCase
{
    Task<Result> ExecuteAsync(ulong runId, ulong postId, AllocatePostRequest request, CancellationToken cancellationToken);
}
