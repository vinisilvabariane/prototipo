using FluentResults;
using IrisWeb.Application.Communication.Argos.Requests;
using IrisWeb.Application.Communication.Argos.Responses;

namespace IrisWeb.Application.Interfaces.Argos;

public interface IListArgosCyclesUseCase
{
    Task<Result<ArgosCyclesResponse>> ExecuteAsync(
        ArgosCyclesQuery query,
        CancellationToken cancellationToken);
}
