using FluentResults;
using IrisWeb.Application.Communication.Matrix.Requests;
using IrisWeb.Application.Communication.Matrix.Responses;
using IrisWeb.Application.Interfaces.Matrix;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;
using IrisWeb.Domain.Rules;

namespace IrisWeb.Application.UseCases.Matrix;

/// <summary>
/// Mesma lista de recomendados que o Sync mostra ao clicar no posto. O Sync pede o id do
/// turno, e o layout so conhece o nome, entao o id sai da lista de turnos do proprio Sync.
/// </summary>
public sealed class ListStationRecommendationsUseCase(
    ISyncMatrixClient sync,
    IFaceEmbeddingRepository embeddings,
    FaceMatchingPolicy policy) : IListStationRecommendationsUseCase
{
    public async Task<Result<IReadOnlyList<MatrixCandidateResponse>>> ExecuteAsync(
        int stationId,
        MatrixStationsQuery query,
        CancellationToken cancellationToken)
    {
        var line = query.Line!.Trim();
        var product = query.Product!.Trim();
        var shift = query.Shift!.Trim();

        var layout = await MatrixMapping.GuardAsync(() => sync.GetLayoutAsync(line, product, shift, cancellationToken));
        if (layout.IsFailed)
            return layout.ToResult();

        var station = MatrixMapping.FindStation(layout.Value, stationId);
        if (station is null)
            return Result.Fail(MatrixErrors.StationNotInLayout(stationId, line, product, shift));

        if (station.ActivityId is not > 0)
            return Result.Fail(MatrixErrors.StationWithoutActivity(station.Id));

        var shifts = await MatrixMapping.GuardAsync(() => sync.ListShiftsAsync(cancellationToken));
        if (shifts.IsFailed)
            return shifts.ToResult();

        var shiftId = shifts.Value
            .FirstOrDefault(candidate => string.Equals(candidate.Name?.Trim(), shift, StringComparison.OrdinalIgnoreCase))
            ?.Id;
        if (shiftId is null)
            return Result.Fail(MatrixErrors.ShiftNotFound(shift));

        var recommended = await MatrixMapping.GuardAsync(() => sync.ListRecommendedAsync(
            station.Id,
            layout.Value.Id,
            station.ActivityId.Value,
            product,
            shiftId.Value,
            cancellationToken));
        if (recommended.IsFailed)
            return recommended.ToResult();

        var enrolled = await MatrixMapping.EnrolledRegistrationsAsync(embeddings, policy, cancellationToken);

        IReadOnlyList<MatrixCandidateResponse> candidates = [.. recommended.Value
            .OrderBy(item => item.Position)
            .Select(item => new MatrixCandidateResponse(
                item.Registration,
                item.Name?.Trim(),
                item.Level,
                item.Function,
                item.Score,
                enrolled.Contains(MatrixMapping.NormalizeRegistration(item.Registration)),
                item.PriorityGroup,
                item.HasValidPostTraining,
                item.AllocatedPostsCount,
                item.RemainingSlots))];

        return Result.Ok(candidates);
    }
}
