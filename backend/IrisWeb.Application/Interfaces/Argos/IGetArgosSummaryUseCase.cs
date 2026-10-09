using FluentResults;
using IrisWeb.Application.Communication.Argos.Requests;
using IrisWeb.Application.Communication.Argos.Responses;

namespace IrisWeb.Application.Interfaces.Argos;

public interface IGetArgosSummaryUseCase
{
    Task<Result<ArgosSummaryResponse>> ExecuteAsync(
        ArgosSummaryQuery query,
        CancellationToken cancellationToken);
}
