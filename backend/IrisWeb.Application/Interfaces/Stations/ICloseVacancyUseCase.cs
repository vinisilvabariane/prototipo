using FluentResults;
using IrisWeb.Application.Communication.Stations.Requests;

namespace IrisWeb.Application.Interfaces.Stations;

public interface ICloseVacancyUseCase
{
    Task<Result> ExecuteAsync(
        long vacancyId,
        CloseVacancyRequest request,
        CancellationToken cancellationToken);
}
