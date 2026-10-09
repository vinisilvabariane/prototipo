using FluentResults;
using IrisWeb.Application.Communication.Argos.Responses;

namespace IrisWeb.Application.Interfaces.Argos;

public interface IListArgosFiltersUseCase
{
    Task<Result<ArgosFiltersResponse>> ExecuteAsync(CancellationToken cancellationToken);
}
