using FluentResults;
using IrisWeb.Application.Communication.Matrix.Dtos;
using IrisWeb.Application.Communication.Matrix.Requests;
using IrisWeb.Application.Communication.Matrix.Responses;
using IrisWeb.Application.Interfaces.Matrix;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;
using IrisWeb.Domain.Rules;

namespace IrisWeb.Application.UseCases.Matrix;

public sealed class AllocateMatrixStationUseCase(
    ISyncMatrixClient sync,
    IFaceEmbeddingRepository embeddings,
    FaceMatchingPolicy policy) : IAllocateMatrixStationUseCase
{
    public async Task<Result<MatrixStationResponse>> ExecuteAsync(
        AllocateMatrixStationRequest request,
        CancellationToken cancellationToken)
    {
        if (!int.TryParse(request.Registration.Trim(), out var registration) || registration <= 0)
            return Result.Fail(MatrixErrors.RegistrationNotNumeric);

        var enrolled = await MatrixMapping.EnrolledRegistrationsAsync(embeddings, policy, cancellationToken);
        if (!enrolled.Contains(MatrixMapping.NormalizeRegistration(registration)))
            return Result.Fail(MatrixErrors.NoFaceEnrollment(registration));

        var line = request.Line.Trim();
        var product = request.Product.Trim();
        var shift = request.Shift.Trim();

        var layout = await MatrixMapping.GuardAsync(() => sync.GetLayoutAsync(line, product, shift, cancellationToken));
        if (layout.IsFailed)
            return layout.ToResult();

        var station = MatrixMapping.FindStation(layout.Value, request.StationId);
        if (station is null)
            return Result.Fail(MatrixErrors.StationNotInLayout(request.StationId, line, product, shift));

        if (station.ActivityId is not > 0)
            return Result.Fail(MatrixErrors.StationWithoutActivity(station.Id));

        var collaborator = await MatrixMapping.GuardAsync(() =>
            sync.GetCollaboratorByRegistrationAsync(registration, cancellationToken));
        if (collaborator.IsFailed)
            return collaborator.ToResult();

        var setup = new SyncSetupCollaborator(
            collaborator.Value.Id,
            layout.Value.Id,
            station.Id,
            station.ActivityId.Value,
            shift,
            product);

        var allocated = await MatrixMapping.GuardAsync(async () =>
        {
            await sync.SetupCollaboratorAsync(setup, cancellationToken);
            return true;
        });
        if (allocated.IsFailed)
            return allocated.ToResult();

        var refreshed = await MatrixMapping.GuardAsync(() => sync.GetLayoutAsync(line, product, shift, cancellationToken));
        if (refreshed.IsFailed)
            return refreshed.ToResult();

        var updated = MatrixMapping.FindStation(refreshed.Value, station.Id) ?? station;

        return Result.Ok(MatrixMapping.ToStation(updated));
    }
}
