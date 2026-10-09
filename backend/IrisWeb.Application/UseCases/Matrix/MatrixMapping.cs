using FluentResults;
using IrisWeb.Application.Communication.Matrix.Dtos;
using IrisWeb.Application.Communication.Matrix.Responses;
using IrisWeb.Application.Errors;
using IrisWeb.Domain.Entities;
using IrisWeb.Domain.Errors;
using IrisWeb.Domain.Interfaces;
using IrisWeb.Domain.Rules;

namespace IrisWeb.Application.UseCases.Matrix;

internal static class MatrixMapping
{
    private const string ServiceName = "Sync";

    private const decimal DefaultStationWidth = 220;

    private const decimal DefaultStationHeight = 140;

    public static async Task<Result<T>> GuardAsync<T>(Func<Task<T>> call)
    {
        try
        {
            return Result.Ok(await call());
        }
        catch (SyncMatrixException exception)
        {
            return Result.Fail(ErrorFor(exception));
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            return Result.Fail(new UpstreamGatewayError($"O {ServiceName} não respondeu: {exception.Message}"));
        }
    }

    public static string? DeviceIdViolation(string? deviceId)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            return MatrixErrors.DeviceRequired;

        return deviceId.Trim().Length > MatrixErrors.MaxDeviceIdLength
            ? MatrixErrors.FieldTooLong("Identificador do aparelho", MatrixErrors.MaxDeviceIdLength)
            : null;
    }

    /// <summary>
    /// Matriculas com cadastro facial no modelo em uso, sem zeros a esquerda: o Sync fala em
    /// numero e o cadastro guarda o texto digitado.
    /// </summary>
    public static async Task<HashSet<string>> EnrolledRegistrationsAsync(
        IFaceEmbeddingRepository embeddings,
        FaceMatchingPolicy policy,
        CancellationToken cancellationToken)
    {
        var registrations = await embeddings.ListRegistrationsAsync(policy.ModelVersion, cancellationToken);
        return [.. registrations.Select(NormalizeRegistration)];
    }

    public static string NormalizeRegistration(string registration)
    {
        var trimmed = registration.Trim().TrimStart('0');
        return trimmed.Length == 0 ? "0" : trimmed;
    }

    public static string NormalizeRegistration(int registration) => NormalizeRegistration(registration.ToString());

    public static SyncLayoutStation? FindStation(SyncLayout layout, int stationId) =>
        layout.Configuration?.FirstOrDefault(station => station.Id == stationId);

    public static DeviceMatrixResponse ToResponse(DeviceMatrixBinding binding) => new(
        binding.Id,
        binding.Line,
        binding.Product,
        binding.Shift,
        binding.StationId,
        binding.StationName,
        binding.UpdatedAt);

    public static MatrixCollaboratorResponse? AllocatedOn(SyncLayoutStation station) =>
        station.CollaboratorRegistration is { } registration
            ? new MatrixCollaboratorResponse(registration, station.CollaboratorName, station.CollaboratorLevel)
            : null;

    public static MatrixStationResponse ToStation(SyncLayoutStation station) => new(
        station.Id,
        (station.Name ?? string.Empty).Trim(),
        station.IsMachine ?? false,
        AllocatedOn(station),
        station.IsQuality ?? false,
        station.IsCritical ?? false,
        station.ActivityId is > 0,
        station.X ?? 0,
        station.Y ?? 0,
        station.Width is > 0 ? station.Width.Value : DefaultStationWidth,
        station.Height is > 0 ? station.Height.Value : DefaultStationHeight,
        station.Rotation ?? 0);

    private static Error ErrorFor(SyncMatrixException exception)
    {
        var message = string.IsNullOrWhiteSpace(exception.Detail) ? exception.Message : exception.Detail;

        return exception.IsRefusal
            ? new Error(message)
            : new UpstreamGatewayError(message);
    }
}
