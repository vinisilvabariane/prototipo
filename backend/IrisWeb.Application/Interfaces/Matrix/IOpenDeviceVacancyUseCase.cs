using FluentResults;
using IrisWeb.Application.Communication.Matrix.Requests;
using IrisWeb.Application.Communication.Stations.Responses;

namespace IrisWeb.Application.Interfaces.Matrix;

public interface IOpenDeviceVacancyUseCase
{
    Task<Result<OpenVacancyResponse>> ExecuteAsync(
        string deviceId,
        OpenDeviceVacancyRequest request,
        CancellationToken cancellationToken);
}
