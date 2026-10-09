using FluentResults;
using IrisWeb.Application.Communication.Matrix.Requests;
using IrisWeb.Application.Communication.Matrix.Responses;

namespace IrisWeb.Application.Interfaces.Matrix;

public interface ILookupStationCandidateUseCase
{
    Task<Result<MatrixCandidateResponse>> ExecuteAsync(
        int stationId,
        int registration,
        MatrixStationsQuery query,
        CancellationToken cancellationToken);
}
