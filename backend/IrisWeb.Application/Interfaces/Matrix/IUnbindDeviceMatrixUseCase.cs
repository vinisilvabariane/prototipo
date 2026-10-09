using FluentResults;

namespace IrisWeb.Application.Interfaces.Matrix;

public interface IUnbindDeviceMatrixUseCase
{
    Task<Result> ExecuteAsync(string deviceId, CancellationToken cancellationToken);
}
