using FluentResults;
using IrisWeb.Application.Communication.Stations.Requests;

namespace IrisWeb.Application.Interfaces.Stations;

public interface ICloseUnknownUseCase
{
    Task<Result> ExecuteAsync(
        long unknownId,
        CloseUnknownRequest request,
        CancellationToken cancellationToken);
}
