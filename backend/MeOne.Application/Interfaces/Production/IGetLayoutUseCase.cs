using FluentResults;
using MeOne.Application.Communication.Production.Responses;

namespace MeOne.Application.Interfaces.Production;

public interface IGetLayoutUseCase
{
    Task<Result<LayoutDetailResponse>> ExecuteAsync(ulong id, CancellationToken cancellationToken);
}
