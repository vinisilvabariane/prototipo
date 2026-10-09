using FluentResults;
using IrisWeb.Application.Communication.Matrix.Requests;
using IrisWeb.Application.Communication.Matrix.Responses;

namespace IrisWeb.Application.Interfaces.Matrix;

public interface IListStationRecommendationsUseCase
{
    Task<Result<IReadOnlyList<MatrixCandidateResponse>>> ExecuteAsync(
        int stationId,
        MatrixStationsQuery query,
        CancellationToken cancellationToken);
}
