using FluentResults;
using MeOne.Application.Communication.Production.Requests;
using MeOne.Application.Communication.Production.Responses;

namespace MeOne.Application.Interfaces.Production;

public interface ICreateLayoutUseCase
{
    Task<Result<LayoutSummaryResponse>> ExecuteAsync(CreateLayoutRequest request, CancellationToken cancellationToken);
}
