using FluentResults;
using IrisWeb.Application.Communication.Collaborators.Responses;
using IrisWeb.Application.Communication.Matrix.Dtos;
using IrisWeb.Application.Errors;
using IrisWeb.Application.Interfaces.Matrix;
using IrisWeb.Domain.Errors;

namespace IrisWeb.Application.UseCases.Collaborators;

internal static class CollaboratorsMapping
{
    private const string ServiceName = "Sync";

    /// <summary>
    /// Consulta a base de colaboradores do Sync. Recusa do Sync (400/404) vira "matrícula não
    /// encontrada"; Sync fora do ar vira <see cref="UpstreamGatewayError"/> (502).
    /// </summary>
    public static async Task<Result<SyncCollaborator>> LookupAsync(
        ISyncMatrixClient sync,
        uint registration,
        CancellationToken cancellationToken)
    {
        if (registration is 0 or > int.MaxValue)
            return Result.Fail(UserErrors.NotInCollaboratorBase);

        try
        {
            return Result.Ok(await sync.GetCollaboratorByRegistrationAsync((int)registration, cancellationToken));
        }
        catch (SyncMatrixException exception) when (exception.IsRefusal)
        {
            return Result.Fail(UserErrors.NotInCollaboratorBase);
        }
        catch (SyncMatrixException exception)
        {
            return Result.Fail(new UpstreamGatewayError(
                string.IsNullOrWhiteSpace(exception.Detail) ? exception.Message : exception.Detail));
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            return Result.Fail(new UpstreamGatewayError($"O {ServiceName} não respondeu: {exception.Message}"));
        }
    }

    public static CollaboratorResponse ToResponse(SyncCollaborator collaborator) => new(
        (uint)collaborator.Registration,
        (collaborator.FullName ?? string.Empty).Trim(),
        collaborator.Function,
        collaborator.BranchOffice);
}
