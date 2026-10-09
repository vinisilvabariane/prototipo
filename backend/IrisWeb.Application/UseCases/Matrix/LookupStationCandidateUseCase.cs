using FluentResults;
using IrisWeb.Application.Communication.Matrix.Requests;
using IrisWeb.Application.Communication.Matrix.Responses;
using IrisWeb.Application.Interfaces.Matrix;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;
using IrisWeb.Domain.Rules;

namespace IrisWeb.Application.UseCases.Matrix;

/// <summary>
/// Busca por matricula do jeito do Sync: devolve o operador com o nivel dele na atividade do
/// posto, para o lider ver antes de confirmar a alocacao.
/// </summary>
public sealed class LookupStationCandidateUseCase(
    ISyncMatrixClient sync,
    IFaceEmbeddingRepository embeddings,
    FaceMatchingPolicy policy) : ILookupStationCandidateUseCase
{
    public async Task<Result<MatrixCandidateResponse>> ExecuteAsync(
        int stationId,
        int registration,
        MatrixStationsQuery query,
        CancellationToken cancellationToken)
    {
        if (registration <= 0)
            return Result.Fail(MatrixErrors.RegistrationNotNumeric);

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

        var found = await MatrixMapping.GuardAsync(() => sync.GetCollaboratorForActivityAsync(
            registration,
            station.ActivityId.Value,
            product,
            shift,
            station.Id,
            layout.Value.Id,
            cancellationToken));
        if (found.IsFailed)
            return found.ToResult();

        var collaborator = found.Value;
        var enrolled = await MatrixMapping.EnrolledRegistrationsAsync(embeddings, policy, cancellationToken);

        return Result.Ok(new MatrixCandidateResponse(
            registration,
            collaborator.Name?.Trim(),
            collaborator.Level,
            collaborator.Function,
            collaborator.Score,
            enrolled.Contains(MatrixMapping.NormalizeRegistration(registration))));
    }
}
