using IrisWeb.Application.Communication.Matrix.Dtos;

namespace IrisWeb.Application.Interfaces.Matrix;

public interface ISyncMatrixClient
{
    Task<IReadOnlyList<string>> ListLinesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SyncConfiguredProduct>> ListConfiguredProductsAsync(
        string line,
        CancellationToken cancellationToken = default);

    Task<SyncLayout> GetLayoutAsync(
        string line,
        string product,
        string shift,
        CancellationToken cancellationToken = default);

    Task<SyncCollaborator> GetCollaboratorByRegistrationAsync(
        int registration,
        CancellationToken cancellationToken = default);

    Task SetupCollaboratorAsync(SyncSetupCollaborator request, CancellationToken cancellationToken = default);

    Task RemoveCollaboratorAsync(SyncRemoveCollaborator request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SyncShift>> ListShiftsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SyncRecommendedCollaborator>> ListRecommendedAsync(
        int stationId,
        int layoutId,
        int activityId,
        string product,
        int shiftId,
        CancellationToken cancellationToken = default);

    Task<SyncCollaboratorInformation> GetCollaboratorForActivityAsync(
        int registration,
        int activityId,
        string product,
        string shift,
        int stationId,
        int layoutId,
        CancellationToken cancellationToken = default);
}
